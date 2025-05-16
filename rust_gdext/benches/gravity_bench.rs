#![feature(custom_test_frameworks)]
#![feature(iter_collect_into)]
#![test_runner(criterion::runner)]

use criterion::{AxisScale, BatchSize, BenchmarkId, Criterion, PlotConfiguration, black_box};
use criterion_macro::criterion;
use glam::Vec3A;
use godot::prelude::*;
use rand::{Rng, SeedableRng, rngs::StdRng};
use rayon::{
    iter::{IntoParallelIterator, ParallelIterator},
    slice::ParallelSliceMut,
};
use rust_gdext::{
    octree::{
        BoundingBox, GravityData,
        morton_based::{self, MortonBasedOctree, MortonEncodedItem},
        old_versions::{insert_based::InsertBasedOctree, partition_based::PartitionBasedOctree},
    },
    physics::gravity::{
        NBodyGravityCalculator, controller::SimulatedBody, direct_summation::DirectSummation,
    },
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
    Criterion::default()
}

fn log_plotter() -> PlotConfiguration {
    PlotConfiguration::default().summary_scale(AxisScale::Logarithmic)
}

#[criterion(custom_criterion())]
fn compute_accelerations(c: &mut Criterion) {
    static GRAV_CONST: f32 = 1.0;

    let mut group = c.benchmark_group("compute_accelerations");
    group.plot_config(log_plotter());

    let sizes = [
        5, 10, 20, 50, 100, 150, 200, 300, 500, 1000, 2000, 5000, 10000, 20000, 50000, 70000,
        100_000,
    ];

    // Direct summation
    for size in sizes.iter().filter(|s| **s <= 10000) {
        let bodies = create_bench_bodies(*size);

        group.bench_function(BenchmarkId::new("direct/sequential", size), |b| {
            b.iter(|| {
                let accelerations = DirectSummation::calc_accs::<false>(GRAV_CONST, &bodies);
                black_box(accelerations);
            });
        });

        group.bench_function(BenchmarkId::new("direct/parallel", size), |b| {
            b.iter(|| {
                let accelerations = DirectSummation::calc_accs::<true>(GRAV_CONST, &bodies);
                black_box(accelerations);
            });
        });
    }

    // Barnes-Hut
    for size in sizes.iter() {
        let bodies = create_bench_bodies(*size);

        group.bench_function(BenchmarkId::new("barnes_hut/sequential", size), |b| {
            b.iter(|| {
                let accelerations = MortonBasedOctree::calc_accs::<false>(GRAV_CONST, &bodies);
                black_box(accelerations);
            });
        });

        group.bench_function(BenchmarkId::new("barnes_hut/parallel", size), |b| {
            b.iter(|| {
                let accelerations = MortonBasedOctree::calc_accs::<true>(GRAV_CONST, &bodies);
                black_box(accelerations);
            });
        });
    }
    group.finish();
}

#[criterion]
fn octree_build(c: &mut Criterion) {
    let mut group = c.benchmark_group("octree_build");
    for size in [10, 100, 1000, 10000, 100000, 1000000].iter() {
        let bodies = create_bench_bodies(*size);
        let bodies_gd = bodies
            .iter()
            .map(|body| GravityData {
                mass: body.mass,
                center_of_mass: body.pos,
            })
            .collect::<Vec<_>>();

        group.bench_with_input(
            BenchmarkId::new("single-threaded", size),
            size,
            |b, &_size| {
                b.iter(|| {
                    let mut octree = InsertBasedOctree::new(&bodies);
                    octree.build();
                    black_box(octree);
                });
            },
        );

        group.bench_with_input(BenchmarkId::new("morton", size), size, |b, &_size| {
            b.iter(|| {
                let octree = MortonBasedOctree::new(&bodies_gd);
                black_box(octree);
            });
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
                        let octants =
                            PartitionBasedOctree::partition_bodies_parallel(&bounds, data);
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
                        let octants =
                            PartitionBasedOctree::partition_bodies_parallel2(&bounds, data);
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
                        let octants =
                            PartitionBasedOctree::partition_bodies_parallel3(&bounds, data);
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
                        let octants =
                            PartitionBasedOctree::partition_bodies_sequential(&bounds, data);
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
    group.plot_config(log_plotter());

    for size in [
        5, 10, 20, 50, 100, 150, 200, 300, 500, 1000, 2000, 5000, 10000, 20000, 50000, 70000,
        100_000,
    ]
    .iter()
    {
        let data = random_gravity_data(&mut rng, *size, 1000.0);
        let bounds = BoundingBox::containing_gravity_data(&data);

        group.bench_with_input(
            BenchmarkId::new("morton-sequential", size),
            &data,
            |b, data_ref| {
                b.iter(|| {
                    let encoded_bodies: Vec<_> = data_ref
                        .iter()
                        .map(|body| {
                            // Pass the calculated cubic bounds to the encode function
                            let code = morton_based::encode(body.center_of_mass, &bounds);
                            MortonEncodedItem {
                                morton_code: code,
                                item: body,
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
                    let encoded_bodies: Vec<_> = data_ref
                        .into_par_iter()
                        .map(|body| {
                            // Pass the calculated cubic bounds to the encode function
                            let code = morton_based::encode(body.center_of_mass, &bounds);
                            MortonEncodedItem {
                                morton_code: code,
                                item: body,
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

        let encoded_bodies: Vec<_> = data
            .into_par_iter()
            .map(|body| {
                // Pass the calculated cubic bounds to the encode function
                let morton_code = morton_based::encode(body.center_of_mass, &bounds);
                MortonEncodedItem {
                    morton_code,
                    item: body,
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
                        data.par_sort_unstable_by_key(|item| item.morton_code);
                        black_box(data);
                    },
                    BatchSize::SmallInput,
                );
            },
        );
    }

    group.finish();
}
