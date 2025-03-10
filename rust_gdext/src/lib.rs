#![feature(adt_const_params)]

mod physics;
pub mod worker;

use godot::prelude::*;
struct MyExtension;

#[gdextension]
unsafe impl ExtensionLibrary for MyExtension {}
