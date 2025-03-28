use glam::Vec3A;

use crate::octree::{Massive, Spacial};

use super::controller::SimulatedBody;

impl Spacial for SimulatedBody {
    #[inline(always)]
    fn get_pos(&self) -> Vec3A {
        self.pos
    }
}

impl Massive for SimulatedBody {
    #[inline(always)]
    fn get_mass(&self) -> f32 {
        self.mass
    }
}
