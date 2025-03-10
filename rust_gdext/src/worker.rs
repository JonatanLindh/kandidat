//! Thread-based background task processing utility.
//!
//! This module provides a generic worker implementation that runs tasks in a background thread
//! and communicates with the main thread via channels. It is designed for long-running or
//! computationally intensive operations that would otherwise block the main thread.
//!
//! # Usage Example
//!
//! ```rust
//! use crate::worker::{Worker, WorkerLifecycle};
//! use std::sync::mpsc::Sender;
//!
//! // Define command types
//! enum CalculationCommand {
//!     Calculate(Vec<f32>),
//!     Shutdown,
//! }
//!
//! // Create a worker with a handler function
//! let worker = Worker::new(|command, result_tx| match command {
//!     CalculationCommand::Calculate(data) => {
//!         // Perform calculation
//!         let result = data.iter().sum::<f32>();
//!         
//!         // Send result back to main thread
//!         if let Err(e) = result_tx.send(result) {
//!             eprintln!("Failed to send result: {}", e);
//!         }
//!         
//!         // Continue processing commands
//!         WorkerLifecycle::Continue
//!     },
//!     
//!     CalculationCommand::Shutdown => WorkerLifecycle::Shutdown,
//! });
//!
//! // Send a command to the worker
//! worker.command_sender.send(CalculationCommand::Calculate(vec![1.0, 2.0, 3.0])).unwrap();
//!
//! // Check for results
//! if let Ok(result) = worker.try_recv() {
//!     println!("Got result: {}", result);
//! }
//!
//! // Shut down the worker when done
//! worker.command_sender.send(CalculationCommand::Shutdown).unwrap();
//! worker.join().unwrap();
//! ```
//!
//! # Handler Function
//!
//! The key component of the `Worker` is the handler function passed to `Worker::new()`.
//! This function:
//!
//! - Receives commands from the main thread
//! - Performs the actual work
//! - Sends results back via the provided channel
//! - Returns a `WorkerLifecycle` (or a type convertible to it) to indicate whether to continue or shut down
//!
//! ## Handler Function Requirements
//!
//! The handler function must:
//!
//! 1. Accept two arguments:
//!    - `command`: A value of type `Command` - represents a task to be performed
//!    - `result_tx`: A `Sender<Output>` - used to send results back to the main thread
//!
//! 2. Return a value that can be converted into a `WorkerLifecycle`:
//!    - Return `WorkerLifecycle::Continue` to keep processing commands
//!    - Return `WorkerLifecycle::Shutdown` to exit the worker thread
//!    - Return `()` (unit) which automatically converts to `WorkerLifecycle::Continue`

use std::{
    any::Any,
    panic::AssertUnwindSafe,
    sync::mpsc::{Receiver, SendError, Sender, TryRecvError, channel},
    thread::JoinHandle,
};

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
///
/// # Thread Safety
///
/// Both `Res` and `Command` must be `Send + 'static` to ensure safe cross-thread communication.
pub struct Worker<Res, Command> {
    /// Thread handle for joining the worker thread
    handle: JoinHandle<()>,

    /// Channel for sending commands to the worker thread
    command_sender: Sender<Command>,

    /// Channel for receiving results from the worker thread
    result_receiver: Receiver<Res>,
}

/// Response from a worker handler function that controls worker lifecycle.
///
/// When returned from a worker handler function, this enum indicates whether
/// the worker thread should continue processing commands or shut down.
#[derive(PartialEq, Eq)]
pub enum WorkerLifecycle {
    /// Continue processing commands
    Continue,
    /// Shut down the worker thread
    Shutdown,
}

// Make () default to Continue for convenience
impl From<()> for WorkerLifecycle {
    fn from(_: ()) -> Self {
        WorkerLifecycle::Continue
    }
}

impl<Res, Command> Worker<Res, Command>
where
    Command: Send + 'static,
    Res: Send + 'static,
{
    /// Creates a new worker with the specified handler function.
    ///
    /// # Parameters
    ///
    /// * `worker_handler` - Function that processes commands and produces results
    ///
    /// # Handler Function
    ///
    /// The worker handler receives two parameters:
    /// * A command of type `Command` to process
    /// * A `Sender<Res>` to send results back to the main thread
    ///
    /// The handler should return a value convertible to `WorkerLifecycle`:
    /// * `WorkerLifecycle::Continue` to keep processing commands
    /// * `WorkerLifecycle::Shutdown` to terminate the worker thread
    /// * `()` (unit) which automatically converts to `WorkerLifecycle::Continue`
    ///
    /// # Example
    ///
    /// ```no_run
    /// let worker = Worker::new(|command, result_tx| match command {
    ///     CalculationCommand::Calculate(data) => {
    ///         let result = data.iter().sum::<f32>();
    ///         result_tx.send(result).unwrap();
    ///         WorkerLifecycle::Continue
    ///     },
    ///     CalculationCommand::Shutdown => WorkerLifecycle::Shutdown,
    /// });
    /// ```
    pub fn new<Handler, S>(worker_handler: Handler) -> Self
    where
        S: Into<WorkerLifecycle>,
        Handler: Fn(Command, Sender<Res>) -> S + Send + 'static,
    {
        let (cmd_tx, cmd_rx) = channel::<Command>();
        let (result_tx, result_rx) = channel::<Res>();

        // Spawn a thread to handle the worker function
        let handle = std::thread::spawn(move || {
            // Use `catch_unwind` to handle panics in the worker thread
            if let Err(e) = std::panic::catch_unwind(AssertUnwindSafe(|| {
                // Loop to receive commands and process them
                while let Ok(command) = cmd_rx.recv() {
                    let response = worker_handler(command, result_tx.clone());

                    // Check if the handler wants to shut down
                    if response.into() == WorkerLifecycle::Shutdown {
                        break;
                    }
                }
            })) {
                // If a panic occurs, handle it here
                let message = e
                    .downcast_ref::<String>()
                    .map(|s| s.as_str())
                    .or_else(|| e.downcast_ref::<&str>().copied())
                    .unwrap_or("unknown error");

                eprintln!("Worker thread panicked: {:?}", message);
            }
        });

        Self {
            handle,
            command_sender: cmd_tx,
            result_receiver: result_rx,
        }
    }

    /// Sends a command to the worker thread.
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
