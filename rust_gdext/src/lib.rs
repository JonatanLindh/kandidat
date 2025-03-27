#![feature(iter_collect_into)]

pub mod physics;
pub mod worker;

use godot::prelude::*;
struct MyExtension;

#[gdextension]
unsafe impl ExtensionLibrary for MyExtension {}
