#![feature(iter_collect_into)]

pub mod octree;
pub mod physics;
pub mod worker;

use godot::prelude::*;
struct MyExtension;

#[gdextension]
unsafe impl ExtensionLibrary for MyExtension {}

pub fn to_glam_vec3(v: Vector3) -> glam::Vec3A {
    glam::Vec3A::new(v.x, v.y, v.z)
}

pub fn from_glam_vec3(v: glam::Vec3A) -> Vector3 {
    Vector3::new(v.x, v.y, v.z)
}
