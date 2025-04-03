#![feature(custom_test_frameworks)]
#![feature(iter_collect_into)]
#![test_runner(criterion::runner)]

use criterion::{BatchSize, BenchmarkId, Criterion, black_box};
use criterion_macro::criterion;
use glam::Vec3A;
use godot::prelude::*;
use rand::{Rng, SeedableRng, rngs::StdRng};
use rayon::{
    iter::{IndexedParallelIterator, IntoParallelRefIterator, ParallelIterator},
    slice::ParallelSliceMut,
};
use rust_gdext::{
    octree::{
        BoundingBox, GravityData, Octree,
        morton::{self, MortonEncodedItem},
        parallel::ParallelOctree,
    },
    physics::gravity::controller::{GravityController, SimulatedBody},
};

const BENCH_SEED: u64 = 20240401;

// Helper function to create a collection of simulated bodies
pub fn create_bench_bodies(n: u32) -> Vec<SimulatedBody> {
    let mut bodies = Vec::with_capacity(n as usize);

    for i in 1..=n {
        // Position bodies in a spherical pattern
        let phi = (i as f32) * 0.1;
        let theta = (i as f32) * 0.2;
        let radius = 1000.0 + (i as f32) * 10.0;

        bodies.push(SimulatedBody {
            body_instance_id: InstanceId::from_i64(i.into()),
            mass: 1000.0 + (i as f32),
            pos: Vec3A::new(
                radius * phi.sin() * theta.cos(),
                radius * phi.sin() * theta.sin(),
                radius * phi.cos(),
            ),
            vel: Vec3A::ZERO,
        });
    }

    bodies
}

fn random_gravity_data(rng: &mut StdRng, n: u32, range: f32) -> Vec<GravityData> {
    let mut rand_n = |range: f32| rng.random_range(-range..range);

    (0..n)
        .map(|_| {
            let x = rand_n(range);
            let y = rand_n(range);
            let z = rand_n(range);
            GravityData {
                mass: rand_n(range),
                center_of_mass: Vec3A::new(x, y, z),
            }
        })
        .collect()
}

fn custom_criterion() -> Criterion {
    Criterion::default().sample_size(20)
}

#[criterion(custom_criterion())]
fn from_elem(c: &mut Criterion) {
    static GRAV_CONST: f32 = 1.0;
    static STEP_DELTA: f32 = 0.01;

    let mut group = c.benchmark_group("Gravity: Single time step");
    for size in [
        10, 50, 100, 200, 350, 500, 750, 1000, 2000, 4000, 7000, 10000,
    ]
    .iter()
    {
        group.bench_with_input(
            BenchmarkId::new("single-threaded", size),
            size,
            |b, &size| {
                let mut bodies = create_bench_bodies(size);
                let mut accelerations = vec![Vec3A::ZERO; size as usize];

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
                let mut accelerations = vec![Vec3A::ZERO; size as usize];

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

#[criterion]
fn octree_build(c: &mut Criterion) {
    let mut group = c.benchmark_group("octree_build");
    for size in [10, 100, 1000, 10000, 100000, 1000000].iter() {
        let bodies = create_bench_bodies(*size);

        group.bench_with_input(
            BenchmarkId::new("single-threaded", size),
            size,
            |b, &_size| {
                b.iter(|| {
                    let mut octree = Octree::new(&bodies);
                    octree.build();
                    black_box(octree);
                });
            },
        );

        group.bench_with_input(BenchmarkId::new("parallel", size), size, |b, &size| {
            let bodies = create_bench_bodies(size);

            b.iter_batched(
                || bodies.iter().map(GravityData::from).collect(),
                |bodies| {
                    let octree = ParallelOctree::build(bodies);
                    black_box(octree);
                },
                BatchSize::LargeInput,
            );
        });
    }
    group.finish();
}

#[criterion]
fn parallel_octree_partition(c: &mut Criterion) {
    let mut rng = StdRng::seed_from_u64(BENCH_SEED);
    let mut group = c.benchmark_group("parallel_octree_partition");

    for size in [10000, 100000, 1000000, 10000000].iter() {
        let data = random_gravity_data(&mut rng, *size, 1000.0);
        let bounds = BoundingBox::containing_gravity_data(&data);

        group.bench_with_input(
            BenchmarkId::new("partition-parallel", size),
            &data,
            |b, data_ref| {
                b.iter_batched(
                    || data_ref.clone(),
                    |data| {
                        let octants = ParallelOctree::partition_bodies_parallel(&bounds, data);
                        black_box(octants);
                    },
                    BatchSize::LargeInput,
                );
            },
        );

        group.bench_with_input(
            BenchmarkId::new("partition-parallel2", size),
            &data,
            |b, data_ref| {
                b.iter_batched(
                    || data_ref.clone(),
                    |data| {
                        let octants = ParallelOctree::partition_bodies_parallel2(&bounds, data);
                        black_box(octants);
                    },
                    BatchSize::LargeInput,
                );
            },
        );

        group.bench_with_input(
            BenchmarkId::new("partition-parallel3", size),
            &data,
            |b, data_ref| {
                b.iter_batched(
                    || data_ref.clone(),
                    |data| {
                        let octants = ParallelOctree::partition_bodies_parallel3(&bounds, data);
                        black_box(octants);
                    },
                    BatchSize::LargeInput,
                );
            },
        );

        group.bench_with_input(
            BenchmarkId::new("partition-sequential", size),
            &data,
            |b, data_ref| {
                b.iter_batched(
                    || data_ref.clone(),
                    |data| {
                        let octants = ParallelOctree::partition_bodies_sequential(&bounds, data);
                        black_box(octants);
                    },
                    BatchSize::LargeInput,
                );
            },
        );
    }
    group.finish();
}

#[criterion]
fn morton_encode(c: &mut Criterion) {
    let mut rng = StdRng::seed_from_u64(BENCH_SEED);
    let mut group = c.benchmark_group("morton_encode");

    for size in [1, 10, 100, 1000, 3000, 5000, 7000, 10000].iter() {
        let data = random_gravity_data(&mut rng, *size, 1000.0);
        let bounds = BoundingBox::containing_gravity_data(&data);

        group.bench_with_input(
            BenchmarkId::new("morton-sequential", size),
            &data,
            |b, data_ref| {
                b.iter(|| {
                    let encoded_bodies: Vec<MortonEncodedItem> = data_ref
                        .iter()
                        .enumerate()
                        .map(|(index, body)| {
                            // Pass the calculated cubic bounds to the encode function
                            let code = morton::encode(body.center_of_mass, &bounds);
                            MortonEncodedItem {
                                code,
                                body_index: index,
                            }
                        })
                        .collect();

                    black_box(encoded_bodies);
                });
            },
        );

        group.bench_with_input(
            BenchmarkId::new("morton-parallel", size),
            &data,
            |b, data_ref| {
                b.iter(|| {
                    let encoded_bodies: Vec<MortonEncodedItem> = data_ref
                        .par_iter()
                        .enumerate()
                        .map(|(index, body)| {
                            // Pass the calculated cubic bounds to the encode function
                            let code = morton::encode(body.center_of_mass, &bounds);
                            MortonEncodedItem {
                                code,
                                body_index: index,
                            }
                        })
                        .collect();

                    black_box(encoded_bodies);
                });
            },
        );
    }

    group.finish();
}

#[criterion]
fn morton_sort(c: &mut Criterion) {
    let mut rng = StdRng::seed_from_u64(BENCH_SEED);
    let mut group = c.benchmark_group("morton_sort");

    for size in [7000, 15000, 50000, 100000].iter() {
        let data = random_gravity_data(&mut rng, *size, 1000.0);
        let bounds = BoundingBox::containing_gravity_data(&data);

        let encoded_bodies: Vec<MortonEncodedItem> = data
            .par_iter()
            .enumerate()
            .map(|(index, body)| {
                // Pass the calculated cubic bounds to the encode function
                let code = morton::encode(body.center_of_mass, &bounds);
                MortonEncodedItem {
                    code,
                    body_index: index,
                }
            })
            .collect();

        group.bench_with_input(
            BenchmarkId::new("morton-sort-sequential", size),
            &encoded_bodies,
            |b, data_ref| {
                b.iter_batched(
                    || data_ref.clone(),
                    |mut data| {
                        data.sort_unstable();
                        black_box(data);
                    },
                    BatchSize::SmallInput,
                );
            },
        );

        group.bench_with_input(
            BenchmarkId::new("morton-sort-parallel", size),
            &encoded_bodies,
            |b, data_ref| {
                b.iter_batched(
                    || data_ref.clone(),
                    |mut data| {
                        data.par_sort_unstable();
                        black_box(data);
                    },
                    BatchSize::SmallInput,
                );
            },
        );

        group.bench_with_input(
            BenchmarkId::new("morton-sort-parallel-key", size),
            &encoded_bodies,
            |b, data_ref| {
                b.iter_batched(
                    || data_ref.clone(),
                    |mut data| {
                        data.par_sort_unstable_by_key(|item| item.code);
                        black_box(data);
                    },
                    BatchSize::SmallInput,
                );
            },
        );
    }

    group.finish();
}
