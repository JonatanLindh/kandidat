#![feature(custom_test_frameworks)]
#![feature(iter_collect_into)]
#![test_runner(criterion::runner)]

use criterion::{BenchmarkId, Criterion};
use criterion_macro::criterion;
use godot::prelude::*;
use rust_gdext::physics::gravity::controller::{GravityController, SimulatedBody};

// Helper function to create a collection of simulated bodies
fn create_bench_bodies(n: u32) -> Vec<SimulatedBody> {
    let mut bodies = Vec::with_capacity(n as usize);

    for i in 1..=n {
        // Position bodies in a spherical pattern
        let phi = (i as f32) * 0.1;
        let theta = (i as f32) * 0.2;
        let radius = 1000.0 + (i as f32) * 10.0;

        bodies.push(SimulatedBody {
            body_instance_id: InstanceId::from_i64(i.into()),
            mass: 1000.0 + (i as f32),
            pos: Vector3::new(
                radius * phi.sin() * theta.cos(),
                radius * phi.sin() * theta.sin(),
                radius * phi.cos(),
            ),
            vel: Vector3::ZERO,
        });
    }

    bodies
}

#[criterion]
fn from_elem(c: &mut Criterion) {
    static GRAV_CONST: f32 = 1.0;
    static STEP_DELTA: f32 = 0.01;

    let mut group = c.benchmark_group("Gravity: Single time step");
    for size in [10, 50, 100, 500, 1000, 10000].iter() {
        group.bench_with_input(
            BenchmarkId::new("single-threaded", size),
            size,
            |b, &size| {
                let mut bodies = create_bench_bodies(size);
                let mut accelerations = vec![Vector3::ZERO; size as usize];

                b.iter(|| {
                    GravityController::step_time_single_core(
                        GRAV_CONST,
                        STEP_DELTA,
                        &mut bodies,
                        &mut accelerations,
                    );
                });
            },
        );

        group.bench_with_input(
            BenchmarkId::new("rayon-parallel", size),
            size,
            |b, &size| {
                let mut bodies = create_bench_bodies(size);
                let mut accelerations = vec![Vector3::ZERO; size as usize];

                b.iter(|| {
                    GravityController::step_time(
                        GRAV_CONST,
                        STEP_DELTA,
                        &mut bodies,
                        &mut accelerations,
                    )
                });
            },
        );
    }
    group.finish();
}
