//! Thread-based background task processing utility.
//!
//! This module provides a generic worker implementation that runs tasks in a background thread
//! and communicates with the main thread via channels. It is designed for long-running or
//! computationally intensive operations that would otherwise block the main thread.
//!
//! # Usage Example
//!
//! ```no_run
//! use crate::worker::Worker;
//!
//! // Define command types
//! enum CalculationCommand {
//!     Calculate(Vec<f32>),
//!     Shutdown,
//! }
//!
//! // Create a worker with a loop that processes commands
//! let worker = Worker::new(|cmd_receiver, result_tx| {
//!     while let Ok(command) = cmd_receiver.recv() {
//!         match command {
//!             // Process commands...
//!         }
//!     }
//! });
//!
//! // Send a command to the worker
//! worker.send_command(CalculationCommand::Calculate(vec![1.0, 2.0, 3.0])).unwrap();
//!
//! // Check for results
//! if let Ok(result) = worker.try_recv() {
//!     println!("Got result: {}", result);
//! }
//!
//! // Shut down the worker when done
//! worker.send_command(CalculationCommand::Shutdown).unwrap();
//! worker.join().unwrap();
//! ```
use std::{
    any::Any,
    panic::{AssertUnwindSafe, UnwindSafe},
    sync::mpsc::{self, Receiver, RecvError, SendError, Sender, TryRecvError, channel},
    thread::JoinHandle,
};

use itertools::{FoldWhile, Itertools};

/// A generic worker that runs tasks in a background thread.
///
/// `Worker<Res, Command>` manages a background thread that receives commands of type `Command`
/// and produces results of type `Res`. Commands are sent to the worker via
/// `command_sender`, and results can be received via `result_receiver`.
///
/// # Type Parameters
///
/// * `Res` - The type of results produced by the worker
/// * `Command` - The type of commands accepted by the worker
pub struct Worker<Res, Command> {
    /// Thread handle for joining the worker thread
    handle: JoinHandle<()>,

    /// Channel for sending commands to the worker thread
    command_sender: Sender<Command>,

    /// Channel for receiving results from the worker thread
    result_receiver: Receiver<Res>,
}

/// Provides methods for receiving commands in a worker thread.
///
/// This struct wraps a command channel receiver and provides methods
/// for receiving commands either individually or in batches.
pub struct CommandReceiver<'a, Command> {
    cmd_rx: &'a Receiver<Command>,
}

impl<'a, Command> CommandReceiver<'a, Command> {
    /// Receives a single command from the worker channel, blocking until one is available.
    ///
    /// # Returns
    ///
    /// * `Ok(Command)` - The received command
    /// * `Err(RecvError)` - If the channel is disconnected
    pub fn recv(&self) -> Result<Command, RecvError> {
        self.cmd_rx.recv()
    }

    /// Receives a batch of commands from the worker channel, blocking until at least one is available.
    ///
    /// This method receives one command, then drains any additional commands without blocking.
    /// The first command is stored in the `head` field, and remaining commands are accessible
    /// through the `tail` iterator.
    ///
    /// # Returns
    ///
    /// * `Ok(CommandBatch)` - A batch containing at least one command
    /// * `Err(RecvError)` - If the channel is disconnected
    pub fn recv_batch(&self) -> Result<CommandBatch<'a, Command>, RecvError> {
        let head = self.cmd_rx.recv()?;
        let tail = self.cmd_rx.try_iter();
        Ok(CommandBatch { head, tail })
    }
}

/// Contains a batch of commands received from the worker channel.
///
/// This struct provides access to multiple commands received in a single
/// operation. It guarantees at least one command (`head`), with potentially
/// more commands available through the `tail` iterator.
///
/// Batching commands allows for optimizations like processing only the most
/// recent command or prioritizing specific command types.
pub struct CommandBatch<'a, Command> {
    /// The first command in the batch (guaranteed to exist)
    pub head: Command,

    /// An iterator over additional commands in the batch (may be empty)
    pub tail: mpsc::TryIter<'a, Command>,
}

impl<'a, Command> CommandBatch<'a, Command> {
    /// Returns the latest command in the batch, consuming the batch.
    ///
    /// This method ignores all commands except the most recent one.
    pub fn latest(self) -> Command {
        self.tail.fold(self.head, |_, cmd| cmd)
    }

    /// Finds the first command matching a predicate, or returns the latest if none match.
    ///
    /// Useful for prioritizing specific command types (like shutdown commands).
    pub fn find_or_latest<F>(mut self, predicate: F) -> Command
    where
        F: Fn(&Command) -> bool,
    {
        self.tail
            .fold_while(self.head, |_, cmd| {
                if predicate(&cmd) {
                    FoldWhile::Done(cmd)
                } else {
                    FoldWhile::Continue(cmd)
                }
            })
            .into_inner()
    }
}

impl<Res, Command> Worker<Res, Command>
where
    Command: Send + 'static,
    Res: Send + 'static,
{
    /// Creates a new worker with the specified loop function.
    ///
    /// # Parameters
    ///
    /// * `worker_loop` - Function that implements the worker's main loop
    ///   and handles command processing
    ///
    /// # Loop Function
    ///
    /// The worker loop receives:
    /// * A `CommandReceiver<Command>` for receiving commands
    /// * A `Sender<Res>` to send results back to the main thread
    ///
    /// The loop is responsible for implementing its own loop structure
    /// and determining when to exit.
    pub fn new<F, S>(worker_loop: F) -> Self
    where
        F: Fn(CommandReceiver<Command>, Sender<Res>) -> S,
        F: UnwindSafe + Send + 'static,
    {
        let (cmd_tx, cmd_rx) = channel::<Command>();
        let (result_tx, result_rx) = channel::<Res>();

        // Spawn a thread to handle the worker function
        let handle = std::thread::spawn(move || {
            Self::catch_unwind(move || {
                worker_loop(CommandReceiver { cmd_rx: &cmd_rx }, result_tx);
            })
        });

        Self {
            handle,
            command_sender: cmd_tx,
            result_receiver: result_rx,
        }
    }

    /// Catches panics in the worker thread and handles them gracefully.
    fn catch_unwind<F>(f: F)
    where
        F: FnOnce() + UnwindSafe,
    {
        if let Err(e) = std::panic::catch_unwind(AssertUnwindSafe(f)) {
            // If a panic occurs, handle it here
            let message = e
                .downcast_ref::<String>()
                .map(|s| s.as_str())
                .or_else(|| e.downcast_ref::<&str>().copied())
                .unwrap_or("unknown error");

            eprintln!("Worker thread panicked: {message:?}");
        }
    }

    /// Sends a command to the worker thread.
    ///
    /// # Parameters
    ///
    /// * `command` - The command to be sent to the worker for processing
    ///
    /// # Returns
    ///
    /// * `Ok(())` - If the command was sent successfully
    /// * `Err(SendError<Command>)` - If the command could not be sent (e.g., if the channel is closed)
    #[inline(always)]
    pub fn send_command(&self, command: Command) -> Result<(), SendError<Command>> {
        self.command_sender.send(command)
    }

    /// Tries to receive a result from the worker without blocking.
    ///
    /// Returns `Ok(result)` if a result is available, or an error if
    /// no results are available or if the channel has been closed.
    ///
    /// # Returns
    ///
    /// * `Ok(Res)` - A result from the worker
    /// * `Err(TryRecvError::Empty)` - No results are currently available
    /// * `Err(TryRecvError::Disconnected)` - The worker thread has terminated
    #[inline(always)]
    pub fn try_recv(&self) -> Result<Res, TryRecvError> {
        self.result_receiver.try_recv()
    }

    /// Tries to receive the latest result from the worker without blocking.
    ///
    /// This method drains all available results from the channel and returns only
    /// the most recent one. If multiple results are available, earlier results are
    /// discarded.
    ///
    /// # Returns
    ///
    /// * `Some(result)` - The latest result from the worker
    /// * `None` - No results are currently available
    #[inline(always)]
    pub fn try_recv_latest(&self) -> Option<Res> {
        self.result_receiver.try_iter().last()
    }

    /// Joins the worker thread, waiting for it to finish.
    ///
    /// This method should be called when the worker is no longer needed
    /// to ensure proper cleanup. Typically called after sending a shutdown command.
    ///
    /// # Returns
    ///
    /// * `Ok(())` - The thread terminated normally
    /// * `Err(...)` - The thread panicked with the given payload
    #[inline(always)]
    pub fn join(self) -> Result<(), Box<dyn Any + Send>> {
        self.handle.join()
    }
}
