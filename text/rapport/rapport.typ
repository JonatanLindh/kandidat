#import "@preview/wordometer:0.1.4": word-count, total-words
#import "@preview/numbly:0.1.0": numbly
#import "@preview/algorithmic:0.1.0"
#import algorithmic: algorithm

#let appendix_numbering

#let appendix(body) = {
show heading: set align(center)
set heading(
numbering: numbly("Appendix {1:A}"),
supplement: [],
hanging-indent: 0pt,
)
counter(heading).update(0)
body
}

#set heading(numbering: "1.")
#show: word-count

#page()[
#set align(center + horizon)
#grid(
rows: (1fr),
image("EN_Black_Chalmers_GU.png"),
[
#set text(size: 30pt)
#text(maroon)[Ord: #total-words]

      Simulating a physics-based procedurally generated galaxy

      //#set text(size: 17pt)
      //Creating Solar Systems of
      //Procedurally Generated Planets
    ],
    [
      #set text(size: 18pt)
      DIT561-VT25-56

      #set text(size: 14pt)
      Jacob Andersson \
      Erik Berglind \
      Anton Frejd \
      William Karlsson \
      Jonatan Lindh \
      Paul Soukup
    ]

)
]

#pagebreak()

#heading(numbering: none)[Abstract]
Abstract...

#heading(numbering: none)[Sammandrag]
Sammandrag...

#pagebreak()
#heading(numbering: none)[Acknowledgements]

Acknowledgements...

#pagebreak()
#outline()

#pagebreak()
#outline(title: "Figures", target: figure)

#set page(numbering: "1")
#counter(page).update(1)

#pagebreak()

= Introduction
Procedural generation offers a way to algorithmically create vast and diverse game worlds without the immense manual effort required to design every detail. Applications of procedural generation range from the random placement of enemies in confined dungeon spaces to the generation of entire universes comprising millions of celestial bodies. Using procedural content generation (PCG) also has the potential to increase re-playability #cite(<PCGNeuroevolution>).

Using PCG algorithms is particularly relevant in the context of space exploration games, where the scale of the universe is inherently beyond manual creation. Creating compelling, varied, and believable planetary systems and galaxies is a challenging problem within this domain. Key issues include the computational efficiency required to generate hundreds to millions of celestial objects, as well as the need to balance performance constraints with the goal of providing a plausible and playable experience.

This project aims to address the technical and creative challenges of building such systems in a computationally efficient manner, while also simulating them within a game engine.

== Purpose <purpose-ref>
The aim of this project is to create a physics-based simulation of a procedurally generated, explorable galaxy.

Each solar system that make up the galaxy will consist of various procedurally generated planets, orbiting a central star. These orbits are governed by a simplified physics simulation based on Newtonian physics. System complexity can vary, ranging from a sun with a single planet, to arrangements with multiple planets and moons.

While procedurally generated, the galaxy will remain consistent and revisitable by ensuring deterministic generation. Different seeds will allow for unique galaxies to be created, while also enabling parts to be generated identically, upon revisit.

== Limitations????
Some limitations for the projects have been set. The physics simulation will be simplified and not necessarily accurate according to real laws of gravity. This does not mean that the program will completely disregard the accuracy of the physics model, but will instead focus on certain aspects that are deemed to be important, such as the orbit of celestial bodies. The system needs to be realistic enough to simulate solar systems, but not realistic to a point where all intricacies of physics will be considered.

// kanske skippa denna iaf :), fick ju tveksamheter i feedbacken på planeringsrapporten om detta
Scenarios such as stellar collisions (e.g. collisions between planets) could be an interesting aspect to explore, although, achieving semi-realistic collisions would require significant effort and is also very improbable in the real world. Such events would also not fit the intended design of what the simulated universe should look like, as the generated solar systems are meant to be, and remain, stable.

Furthermore, the project will not be developed into a full scale video game. Rather, the focus will lie on the more technical aspects: terrain generation, physics, and performance optimization. Other features such as a UI for quick travel or detailed planet properties are not prioritized. Essentially, the goal of the project is to create a model of a procedurally generated galaxy.

== Contribution???

= Background
This section presents the foundational theoretical concepts that were needed before beginning the project, followed by a few select previous works that utilize these concepts.

== Procedural Content Generation
Procedural Content Generation (PCG) is defined as “the algorithmic creation of game content with limited or indirect user input”@shaker2016procedural. In video games, this typically involves the automatic generation of content such as unique levels for each gameplay session or the stochastic placement of environmental elements like vegetation.

== Noise
== Terrain Generation
This subsection provides an overview of height maps and the marching cubes algorithm, the two techniques used to procedurally generate terrain in the project.

=== Height maps
Height maps are used as a data structure that stores the elevation for each vertex in a mesh’s geometry@heightmaps:2019. They are usually stored as gray scale image files, where the brightness of each pixel represents the height of the corresponding vertex in the mesh. However there are downsides to using height maps as a they can only store a single elevation and thus cannot have complex terrain such as overhangs.

=== Marching Cubes
Marching cubes@marchingcubes:1998 is a algorithm which can construct a mesh out of an isosurface from a three-dimensional scalar field, or in simpler terms it get an input of values and outputs a mesh from it. It was originally used for visualizing data from CT and MRI devices but has "recently" seen use in procedural mesh generation.

The algorithm works by marching through this scalar field and taking eight neighbors forming a "logical" cube and then determines the polygon for this part of the scalar field. After the polygon is constructed we move on and check the next set of eight neighbors. This continues through the whole scalar field until the end where all the polygons are fused into the mesh.

The polygons are created by treating the eight neighboring points as "inside" or "outside" surface, this can also be called the "iso-level" meaning that if the point is higher than the "iso-level" then it is inside and vice versa. Once all the neighboring points are checked we use a pre calculated lookup table for figuring out the triangulations, since there are eight points which have two states we have $2^8 = 256$ ways of constructing the polygons, although there are only 15 original forms it can take and all the others are a mirrored from any of the 15 forms.
#figure(
image("MarchingCubesEdit.svg"),
caption: [Ryoshoru, #link("https://creativecommons.org/licenses/by-sa/4.0")[CC BY-SA 4.0], via Wikimedia Commons]
)

== Chunks
=== Octrees

== GPU Computation

== Godot
=== Node3Ds, scenes, etc

== Previous works (WIlliam skriver)
Several existing games and research projects provide a foundation for this work, demonstrating both the potential and the challenges of procedural planet and solar system generation:

=== Minecraft
While not focused on planetary systems, Minecraft@minecraft:2009 demonstrates the power of procedural generation for creating vast and varied landscapes using noise. This random generation results in virtually endless environments, drawing players in with the offer of exploring new, never seen before areas. Minecraft's success highlights the appeal of procedurally generated worlds, and the endless possibilities they offer for creativity and exploration.

#figure(
image("images/PreviousWorks/minecraft_landscape.png", width: 65%),
caption: [Minecraft Windswept Hills biome]//@minecraft_landscape]
)

=== No Man's Sky
No Man's Sky@hello_games:2016 famously utilized procedural generation to create a universe of planets, each with unique flora, fauna, and landscapes. It demonstrated the potential of large-scale procedural planet generation in a commercial game.

#figure(
image("images/PreviousWorks/nomanssky.png", width: 65%),
caption: [No Man's Sky planet surface]//@nomanssky_planet]
)#text(red)[källan]

=== Outer wilds? Vår examinator gillar Outer wilds. +1p
...

=== Exo Explorer
Exo Explorer@exo_exporer:2023 is an earlier bachelor thesis project, also from Chalmers, directly addressed the challenge of procedurally generating solar systems using the Unity engine. The project utilized Perlin noise @perlinnoise:1985 and the marching cubes algorithm @marchingcubes:1998 to create planet terrain featuring forests, lakes, and creatures with basic simulated needs (hunger, thirst, reproduction).

Exo Explorer will serve as a valuable source of inspiration for this project, demonstrating techniques of procedural generation, optimization, etc, that this project aims to explore as well. Whilst aiming to delve deeper into other aspects such as the complexity of the simulated physics, performance, and exploration; especially with a greater focus on simulating several solar systems at the same time for the user to explore.

#text(red)[bild?]

= Method and Planning (Mycket kopierat från planeringsrapporten)
== Tools
=== Git (INTE KLAR)
During the development process, the version control system Git will be utilized in conjunction with GitHub. The GitHub repository will also serve as a platform for task management by employing a Kanban board to facilitate tracking of task assignments and progress.

The projects standard workflow for Git and GitHub involves maintaining each feature within a dedicated branch. GitHub’s Kanban board allows us to associate branches with specific tasks, ensuring a clear and structured development process.

Additionally, acceptance criteria will be established for each task on the Kanban board. Once all criteria are met, a pull request will be created to merge the changes into the main branch. Before finalizing the merge, at least one other team member will be required to review the code and the feature. This review process serves both as a quality assurance measure and as an opportunity to provide constructive feedback.

=== Godot (INTE KLAR)
The Godot game engine is the engine that was chosen for this project.

Godot is a free and open-source game engine recognized for its lightweight architecture, efficient scene system, and user-friendly interface. It employs a node-based system that enables modular and reusable component design. The engine officially supports multiple programming languages, including GDScript, C\#, and C++. @GodotFeatures Furthermore, community-driven extensions, detailed in @GDExtension, expand language compatibility beyond these officially supported options.

To minimize conflicts during the merging process in Git, concurrent modifications to the same scene will be limited. This practice helps prevent merge conflicts, which can otherwise introduce inefficiencies and complications in development.

Godot was chosen over other engines because it is a light weight engine compared to others (such as Unity and Unreal Engine); it does not have heavy pc requirements which means that it can easily work on lower end machines.
Additionally, its support for multiple languages gives support to more performance efficient languages such as C++ or Rust, which can help with optimization.
Lastly, as a relatively new engine, it was deemed to be worth learning from an academic perspective.

== Workflow
Development will follow an agile adjacent workflow @Agile101, meaning that the work will be divided into "sprints", and there will be iterative task refinement. Task prioritization and adding tasks to the backlog will be done during the end of the week before the weekly supervisor meeting. This means that there will be new tasks in the backlog, and perhaps a different prioritization, every week. Task management will involve tracking various states for each task (on the Kanban board), including "blocked", "todo", "in progress", "in review", and "done". All labels are self-explanatory except for "blocked"; tasks categorized under this label cannot be worked on before prerequisite task(s) are finished.

=== Performance (SKA MAN HA DETTA HÄR I METOD? :thinking_emoji:)
When developing real-time applications such as simulations, video games, or other computer applications, maintaining responsiveness and stability during runtime is essential for the user experience. A common metric to measure the performance of any such application is Frames Per Second (FPS), which is the amount of rendered images (frames) that are displayed each second. Higher and consistent FPS is desirable for a stable experience, as well as reduced visual artifacts, and improved system latency from when a user inputs, to it being represented on the display. @nvidia_fps

However, FPS alone does not always provide a complete picture of performance. Instead, the time it takes to render each frame (frame times) is considered instead. Frame times reveal inconsistencies during runtime, such as brief momentary lag at computation heavy moments. These moments may be overlooked in average FPS values, while being detrimental to the user experience. Metrics such as 1% lows and 0.1% lows have become common to expose these worst-case scenarios, capturing the average of the slowest (highest value) 1% and 0.1% of all frame times respectively.

The disparities between the 3 values of: the total frame time average, the 1% lows, and the 0.1% lows, are what is important. Reducing the disparities between each other is what is crucial for an overall stable user experience. Gamers Nexus@gamers_nexus_youtube_fps_lows mentions that disparities between frames of 8ms or more, are what is starting to become perceptible to the user.

This approach is detailed in NVIDIA's developer guides@nvidia_frametimes, explained by Gamers Nexus@gamers_nexus_youtube_fps_lows, employed in benchmarks by Gamers Nexus@gamers_nexus_dragons_dogma_benchmark, with underlying work in "Inside the Second" by Scott Wasson@techreport_inside_the_second.

All in all, this project will put emphasis on maintaining consistent frame times, rather than high FPS, more precisely:

- Keep the disparities between the frame time average, the 1% lows, and the 0.1% lows, to a maximum of 8ms.
- Maintain an average 30 FPS (frame times of 33.33ms), when the program is not experiencing its 1%, or 0.1% lows.
  This, on a dedicated benchmarking computer of the following specifications:

SPECS

== Planning
== Task
== Societal and Ethical aspects (VAR SKA DENNA LIGGA) Svar: I diskussionsdelen :), finns en förberedd rubrik för det redan! ERIK KLAR
The following points of discussion regarding ethical and societal aspects that are deemed to be relevant are how procedural content generation in game development affects game designers, mainly focusing on level designers, and how players might be affected by procedural content generation in games.

Game designers within game development might lose their relevance if the procedural generation and the use of AI gets precise enough, meaning that the algorithms can perfectly replace human developers. Even though procedural content generation can help game companies reduce development cost and time#cite(<computers13110304>), the concerns that the algorithms proficient enough to replace human creativity are still present. An example for this is when the Swedish game company Mindark announced plans to fire half of their employees, primarily world builders, in favor of AI-driven content generation#cite(<MindarkAftonbladet>).

The procedural content generation must be interesting enough and playable to not negatively affect players. Games containing procedural content generation are at the risk of containing repetitive content, which may influence a player's sense of immersion or reduce re-playability. An example where the content generation affected the game play negatively is when the game "No Man's Sky" was released . The planets generated by the game ended up being too repetitive and basic @pcgchallanges:2017. Additionally, PCG systems may inadvertently create environments that hinder gameplay, such as untraversable terrain, thereby negatively affecting the overall playability of the game.

= Process
This section outlines the process for creating the various components that comprise the project. Each subsection represents a step of decreasing scale - starting from the galaxy-scale distribution of stars, narrowing down to the system-scale organization of celestial bodies and their orbital physics, and finally reaching the planet-scale, focusing on unique terrain generation and other planetary features.

== Galaxy

A galaxy@galaxy-term is...
// under en "background"-rubrik istället?

blabla

The galaxy underwent multiple iterations.

// Kanske passar någon annanstans
=== Deterministic generation

//////////// källor o grejs...

Deterministic generation, or "seeded" generation, is a way to predetermine the "random" values that a random number generator will produce. This is desirable since a goal of this project is to ensure deterministic generation, such that the resulting generation should always be the same as long as the seed that has been used is the same.

All iterations of the galaxy utilizes an arbitrary integer seed to influence the random number generator, in order to produce the same results whenever the same seed is used. The term "randomly" is used loosely, as it refers to this deterministic process.

=== Star field <star-field-ref>
The first version was a three-dimensional star field, as can be seen in @star-field-img. Points were sampled randomly within a cube to determine the location of star placement. The stars are made up out of a single circular mesh, @star-img. This iteration of the galaxy was finite in scale.

#figure(
image("images/Galaxy/star_field.PNG", width: 60%),
caption: [Star field],
) <star-field-img>

#figure(
image("images/Galaxy/star.PNG", width: 60%),
caption: [Star mesh],
) <star-img>

=== Disc galaxy <disc-galaxy-ref>
Shortly thereafter, a version to imitate a disc galaxy formation was created. The implementation was based on a slightly modified star field from @star-field-ref. Rather than sampling random points from within a cube, they were sampled from within a sphere. Depending on each samples height position, the likelihood of a star being placed decreased from further away from the center of the galaxy. This combined, resulted in a disc shape as well as a concentration of stars towards the center, as can be seen in @disc-galaxy-img.

#figure(
image("images/Galaxy/disc_galaxy.PNG", width: 60%),
caption: [Disc galaxy],
) <disc-galaxy-img>

=== Skybox
A traditional skybox was created in Blender@blender @blender-youtube to serve as a background, primarily to be used for the half-time presentation of the project when visiting a solar system. Unlike the procedurally generated star fields, the skybox does not contain actual 3D stars. Instead, it consists of a pre-rendered image designed to imitate a galaxy of stars. As shown in @skybox-testing-img.

#figure(
image("images/Galaxy/skybox_testing_environment.PNG", width: 60%),
caption: [Skybox testing environment],
) <skybox-testing-img>

As mentioned, this approach was primarily used for presentation but also testing purposes. Since the final aim is a star backdrop composed of actual stars that can be explored, as in the other galaxy iterations. So this implementation won't necessarily be used in the future.

=== Infinite galaxy <infinite-galaxy-ref>
This version is based on the original star field concept from @star-field-ref, this time, extending infinitely in all directions rather than being limited to a confined structure. Stars are distributed procedurally using a seeded random generator. The result can be seen in @infinite-galaxy-img.

Additionally, star placement is now influenced by sampling a noise texture, which can help create formations of stars rather than purely random distributions. These star arrangements can result in regions of higher or lower concentrations of stars, making the galaxy more varied.
// källor för noise o liknande? antagligen så har vi förstås beskrivit det i ett "bakgrunds"-kapitel.
// fin bild på noise? 3D

#figure(
image("images/Galaxy/infinite_galaxy.PNG", width: 70%),
caption: [Infinite galaxy],
) <infinite-galaxy-img>

The galaxy is also chunked, allowing generation of stars in the player's closest vicinity, while culling chunks that are further away. This allows the galaxy to be infinitely explorable, with new chunks of stars generating as the player moves through space. An example of a "Star chunk" can be seen in @star-chunk-img.

#figure(
image("images/Galaxy/star_chunk.PNG", width: 70%),
caption: [Star chunk],
) <star-chunk-img>

=== Finite physics-based galaxy
...........
An Infinite galaxy is cool and all, but calculating and applying physics to stars of an infinite ever-expanding galaxy isn't doable. With great advancements in the physics engine @physics-engine-ref, an attempt to simulate physics of a finite disc-shaped galaxy was performed. Initially, to test its feasibility, it was done by dusting off the disc galaxy implementation from @disc-galaxy-ref. It was retrofitted with...
-Changed to multimesh here as well.
-Star finder refactored to work with octree's, as well as for moving stars // perhaps
....

----...

== Galaxy map (Potentiellt döpa om den här rubriken och skriva till mer, för att göra om den till en slags slutsats istället?) <galaxy-map-ref>
The Galaxy Map represents the first connected galaxy implementation, bringing together various separate components of the project into something cohesive. It accumulates all prior work of the Infinite galaxy (@infinite-galaxy-ref), as well as connects with the implementations of solar systems, orbits, player/camera controls, marching cubes, and planets.

=== Selectable stars <selectable-star-ref>
To enable interaction with individual stars, a new type of selectable star was implemented. Players can now hover over a star with the mouse cursor and click to select it. This was achieved by adding a spherical collision shape to the star object, which detects mouse input events within the collider. This star, and its collider, is shown in @selectable-star-img.

#figure(
image("images/Galaxy/selectable_star.PNG", width: 60%),
caption: [Selectable star],
) <selectable-star-img>

Infinite galaxy (@infinite-galaxy-ref) was developed to allow for distribution of any Godot Node3D scene passed to it, and not only the original star implementation. With this, the star was exchanged for the new selectable star without issues.

To indicate that a star has been selected, the star's location in space is highlighted, together with a distance measured in "Light years" (LYs). This can be seen in the center of @galaxy-map-img. In addition, the coordinates and unique seed of the star is displayed in the bottom-right corner.

=== Navigation
Two distinct modes of transportation have been implemented for navigating the galaxy map.

1. Manual movement: The player can freely move using the player controls, of --//ref player controls?.
2. Fast travel: Once a star is selected, press the "->"-button in the bottom-right of @galaxy-map-img. This moves the player rapidly towards it, stopping a short distance away.

To explore the solar systems themselves, the "Explore"-button in the bottom-right of @galaxy-map-img, can be used to enter the star/solar system currently selected. When pressed, a solar system is generated based on the selected star's seed and transitions the player into it. This system exists in a separate scene from the Galaxy Map.

/// även nämna hur när man går iväg från planeter inne i solsystem så går man ut o tillbaks till galaxy map?
/// samt navigeringen (bara flyg atm) runt planeterna i solsystemen.

=== Seed <seed-ref>
The galaxy utilizes a unique "Galaxy Seed", the same used in @infinite-galaxy-ref, to deterministically generate the placement of stars. With the implementation of explorable solar systems, a need arose to generate new seeds for each system. Were they to utilize the same seed, all solar systems would be identical.

To address this, a custom hash function was developed, allowing for the generation of unique, deterministic, seeds for each star. This function takes into account both the initial Galaxy Seed, and the X, Y, and Z coordinates of a star's position, to produce a star-specific seed. This new seed is then propagated into the stars generation algorithm, which results in unique solar systems while still ensuring deterministic consistency.

// källor för hash-functions?
// implementationsdetaljer? nja. kanske en fin bild på ngt vis som bara visar hur nya seeds genereras?

#figure(
image("images/Galaxy/galaxy_map.PNG", width: 80%),
caption: [Galaxy map],
) <galaxy-map-img>

=== Multi-Meshed Stars & Star Finder
As the scale of the galaxy expanded, performance issues began to surface. In particular, stuttering upon loading new chunks. Whenever the player would reach the border of a chunk (@star-chunk-img), chunks would cull, and new ones would generate.

Since the instancing of hundreds or thousands of new stars made up the bulk of the operations at that time, the theory was that it was that which caused the stutters to occur. With each star possessing a MeshInstance3D@godot-meshinstance3d, and a collider (as introduced in @selectable-star-ref).

To address this, the rendering of stars was refactored to utilize Godot's MultiMeshInstance3D@godot-multimeshinstance3d. This change significantly reduced the amount of nodes instantiated in the scene, as well as the draw calls to the GPU, from one for each star to only one for each chunk (containing hundreds of stars).

This would see performance improvements from
//--- frametimes?
///
...

However, this change introduced a new challenge. Since multi-mesh only instances visual meshes, and not other objects such as colliders, the stars were no longer selectable. To reintroduce star selection, two approaches were considered:

1. _Instantiate colliders at star positions_: Instantiate only a collider at each star position, but still render the meshes with the multi-mesh implementation.

2. _Ray-based selection - The "Star Finder"_: Use the known positions of stars in space, and when the player clicks, cast a ray in that direction. At regular intervals along the ray, check the surrounding area for any star positions falling within a set radius of the ray.

The second option was implemented as a system called "Star Finder", which again allows for interaction with stars, despite them only consisting of a visual mesh. Achieved via ray-casting and distance checks to the ray at regular intervals, iterating through the array of star positions of the current chunk. The Star Finder can be seen in action in @star-finder-img.

The first option would have allowed for simpler logic in star selection, but would also include a greater load on Godot's collider calculations, as well as keep the scene hierarchy filled with hundreds/thousands of instantiated colliders. The instancing of these colliders themselves would have likely contributed to performance deterioration, since the large amount of instancing of nodes were suspected to be the cause of the stuttering from the start.

#figure(
image("images/Galaxy/star_finder.PNG", width: 80%),
caption: [Star Finder],
) <star-finder-img>

The Star Finder was implemented with modularity in mind, with multiple parameters that can be tweaked to modify its behavior. In the image above, the interval and radius of proximity checks (the blue spheres) are very regular in order to not miss any stars. The radius of each check also increases the further from the start position it gets, to make selection of distant stars easier.

== Physics Engine <physics-engine-ref>
Simulating the gravitational interactions within a galaxy, containing potentially thousands or millions of stars and planets, presents a significant computational challenge known as the N-body problem. The goal is to calculate the net gravitational force acting on each body at discrete time steps and use this information to update their positions and velocities over time. This section details the progression of methods implemented to tackle this problem within our project.

=== Direct Summation
The most straightforward approach to solving the N-body problem is the direct summation method. This technique relies directly on Newton's Law of Universal Gravitation #text(red)[CITE NEWTON], which states that the gravitational force F between two point masses $m_1$ and $m_2$ separated by a distance $r$ is proportional to the product of their masses and inversely proportional to the square of the distance between them:
$
  F = G (m_1 m_2)/r^2
$

Since directions are needed, we have to express this in vector form:
$
  F_12 = G (m_1 m_2)/(|r_12|^3) r_12
$

where:
$
  & r_12 = r_2 - r_1 
  && "is the vector pointing from particle 1 to particle 2"
\
  & G 
  && "is the gravitational constant"
$

To find the total force (or acceleration, $a = F / m$) acting on a particle $i$, the direct summation method calculates the force vector from every other particle $j != i$ and sums them up:
$
  a_i = sum_(j != i) G m_j/(|r_(i j)|^3) r_(i j)
$

This requires calculating the interaction between every pair of particles in the system. For N particles, this results in $N(N−1)/2$ pairwise force calculations per time step. The computational complexity is therefore $O(N^2)$.

Our full implementation of this method can be found in `rust_gdext/src/physics/gravity/direct_summation.rs`. The core logic involves nested loops iterating through all particle pairs:

#box[
#algorithm({
import algorithmic: \*
Function("Direct-Summation-Gravity", args: ("G", "particles"), {
Cmt[Get the number of particles]
Assign[$n$]#FnI[length][particles]]

    State[]
    Cmt[Initialize acceleration array]
    Assign[acc][#FnI[array][#FnI[Vec3][0, 0, 0]\; $n$]]

    State[]
    Cmt[For each particle $i$, calculate acceleration from each particle $j != i$]
    For(cond: [$i$ *in* $0..n$], {
      For(cond: [$j != i$ *in* $0..n$], {

        Assign[$r_(i j)$][particles[j].*position* - particles[i].*position*]
        Assign[$m_j$][particles[j].*mass*]

        State[]
        If(cond: [$|r_(i j)|> 0$], {
          Assign[$a_(i j)$][$G m_j/(|r_(i j)|^3) r_(i j)$]
          Assign[acc[i]][acc[i] $+ a_(i j)$]

        })
      })
    })

    Return[acc]

})
})
]

#text(red)[IMAGE: Diagram illustrating pairwise force calculation between N bodies.]

While simple and accurate for small numbers of bodies, the $O(N^2)$ complexity makes the direct summation method computationally prohibitive for simulating large systems like galaxies or even dense star clusters within our target performance goals.

=== Barnes-Hut Approximation
To overcome the limitations of the direct summation method for large $N$, approximation techniques are necessary. The Barnes-Hut algorithm @Barnes_Hut_1986 provides a significant performance improvement by reducing the complexity to $O(N "log"N)$.

The core idea is to group distant particles together and treat them as a single, larger particle located at their collective center of mass (CoM). This approximation is valid because, according to Newton's shell theorem, the gravitational effect of a spherically symmetric mass distribution outside the sphere is the same as if all the mass were concentrated at its center. While galactic structures aren't perfectly symmetric, the approximation holds reasonably well for distant groups.

==== Octree Data structure #text(red)[MAX TRE RUBRIKNIVÅER]
The Barnes-Hut algorithm utilizes an _octree_ to hierarchically partition the 3D space occupied by the particles. An octree is a tree data structure where each internal node represents a cubic region of space and has exactly eight children, corresponding to the eight octants of that region @Meagher_1982. This structure allows the algorithm to efficiently group particles based on their spatial location.

Our implementation (`rust_gdext/src/octree/morton_based.rs`) employs a specific technique for building the octree, leveraging _Morton codes_ (also known as Z-order curve codes #text(red)[CITE MORTON]) to facilitate efficient construction from the particle data.

- _Morton Codes:_ A Morton code is a single integer value derived by interleaving the bits of a point's coordinates (x, y, z) after they have been scaled to integer values representing their position within a defined bounding box. For our 64-bit Morton codes (`rs type MortonCode = u64`), we can represent up to 21 bits per dimension (`rs const MAX_DEPTH: u32 = 21`), providing sufficient precision for partitioning space. The `encode` function performs this process:

  1.  Normalizes the particle's `Vec3A` position relative to the minimum corner of the overall bounding box, scaling it to the range $[0, 1]$.
  2.  Scales these normalized coordinates to integer grid coordinates within the range $[0, 2^("MAX_DEPTH")-1]$.
  3.  Interleaves the bits of the resulting unsigned integer coordinates $(x, y, z)$ using helper functions like `spread_bits` to produce the final 64-bit Morton code.

  The key property of Morton codes is _spatial locality preservation_: points that are close together in 3D space tend to have Morton codes that are close in value (or share a long common prefix).
  #text(red)[IMAGE: Diagram showing 2D AND 3D points and how their bits are interleaved to form a Morton code. Maybe show the Z-order curve path.]

- _Sorting:_ Before building the tree, we calculate the Morton code for every particle and store it along with the particle's original index in a `MortonEncodedItem<usize>` struct. We then sort this list of items based _only_ on the Morton codes. This sorting step is crucial, as it arranges the particles in a 1D sequence that largely reflects their 3D spatial proximity.

  - _Parallelization:_ This sorting step is parallelized using `rayon::par_sort_unstable` for efficiency, especially beneficial for large numbers of particles. The initial Morton code calculation using `encode` is also parallelized above a certain threshold (`PARALLEL_ENCODE_THRESHOLD`).
    #text(red)[IMAGE: Visualization comparing random particle distribution vs. the 1D sorted list, showing how spatially close particles end up near each other in the list.]

- _Tree Construction (`build_recursive`):_ With the particles sorted by Morton code, the octree hierarchy can be constructed efficiently. Our `build_recursive` function operates on ranges (`body_range`) within this sorted list:
  1.  _Base Cases:_ If a range contains only one particle (`count == 1`) or the maximum tree depth (`MAX_DEPTH`) is reached, a leaf `Node` is created. Its `GravityData` (mass, CoM) is calculated directly from the particle(s) referenced within its `body_range`.
  2.  _Internal Nodes:_ If the range contains multiple particles and is below the maximum depth, an internal `Node` is created (initially as a placeholder).
  3.  _Partitioning:_ The core of the construction relies on the sorted Morton codes. To determine which particles belong to which of the 8 child octants, the function doesn't need to perform expensive geometric tests. Instead, it uses `find_octant_split`. This function leverages the sorted order and performs a binary search (`partition_point`) within the current `body_range`. It checks the relevant 3 bits of the Morton codes at the `current_depth` (using `get_octant_index_at_depth`) to efficiently find the exact indices (`split_indices`) where the particles transition from belonging to one octant to the next. This partitioning is highly efficient due to the sorted input.
      // IMAGE SUGGESTION: Diagram illustrating how find_octant_split partitions a section of the sorted list based on Morton code bits for a specific depth level.
  4.  _Recursion:_ The `build_recursive` function is then called for each non-empty child range identified by the split indices.
  5.  _Aggregation:_ After the recursive calls return (meaning all descendant nodes have been created and their `GravityData` calculated), the internal node aggregates the mass and calculates the combined center of mass from the data returned by its direct children. The placeholder node created earlier is then finalized with this aggregated data and the child indices.

The resulting `rs MortonBasedOctree` structure contains the `rs Vec<Node>` holding all the nodes, the `sorted_indices` list mapping ranges back to original particle indices, and the `root_index` needed to start the Barnes-Hut traversal. This structure, built efficiently using Morton codes and parallel sorting, provides the necessary spatial hierarchy and aggregate data for the $O(N "log"N)$ Barnes-Hut force calculation.

#box[
The `Node` struct itself contains the necessary information for the Barnes-Hut algorithm:

```rs
  struct Node {
    /// The bounding box of the node.
    bounds: BoundingBox,

    /// Indexes of this nodes children
    children: [Option<NonZeroUsize>; 8], // Changed type slightly for clarity

    /// The range of indices in the sorted_indices list that fall within
    /// this node's spatial region.
    body_range: Range<usize>,

    /// Depth level of the node
    depth: u32,

    /// The data associated with this node. (Total mass and CoM)
    pub data: GravityData,
  } // cite: 1
```

]

1. _Root Node:_ Represents the bounding box containing all particles.

   2. _Subdivision:_ If a node contains more than one particle, its region is subdivided into eight smaller cubic octants, and corresponding child nodes are created. Particles are placed into the appropriate child node based on their position. This process continues recursively until each leaf node contains at most one particle, or a maximum tree depth is reached.

   3. _Aggregate Properties:_ During or after construction, each internal node calculates and stores the total mass and the center of mass (CoM) of all particles contained within its spatial region (i.e., all particles in its descendant leaves).

   #text(red)[ IMAGE: Diagram showing the final octree structure (nodes as cubes/boxes) overlaid on the particle distribution. Maybe highlight one internal node and its CoM/bounds, and one leaf node and its particle range.]

==== Force Calculation with Barnes-Hut
Once the octree is built, the acceleration on a target particle $i$ is calculated by traversing the tree starting from the root node. For each node encountered during the traversal, the algorithm applies the _Multipole Acceptance Criterion (MAC)_:

    1. Calculate the distance $d$ from the target particle $i$ to the center of mass of the current node $n$.

    2. Determine the spatial size $s$ of the node $n$ (i.e., the width of its bounding box).

    3. Compare the ratio $s/d$ to a predefined threshold parameter $theta$ (typically between 0.5 and 1.0). The criterion is usually checked as $s/d < theta$ or, more efficiently, $s^2 < theta^2 d^2$.

The traversal proceeds as follows:

    1. *If $s^2 < theta^2 d^2$* (Node is "Far Enough"): The node's spatial extent is small enough relative to its distance from the target particle. The gravitational contribution of all particles within this node is approximated by a single interaction between the target particle $i$ and the node's total mass acting at its center of mass. The recursion down this branch stops.

    2. *Else* (Node is "Too Close"): The approximation is not sufficiently accurate.
        - *If the node is an internal node:* The algorithm recursively visits each of the node's non-empty children.

        - *If the node is a leaf node:* The algorithm calculates the direct gravitational interaction between the target particle $i$ and each _other_ particle $(i != j)$ actually contained within that leaf node. This requires accessing the original particle data using the indices stored in the leaf node's `body_range`.

Note: A softening factor epsilon is introduced during force calculation (by replacing $d^2$ with $d^2+epsilon^2$) to prevent infinitely large forces when particles get very close, which would destabilize the numerical integration.

The core recursive logic is implemented in the `calculate_accel_recursive` method within `rust_gdext/src/physics/gravity/barnes_hut.rs`. @bh_pseudo contains Pseudocode of the algorithm.

#text(red)[MAYBE IMAGE: Diagram illustrating the Barnes-Hut tree traversal for a single particle. Show a particle, the octree nodes, and highlight which nodes are treated as CoM (MAC passes) and which nodes are opened/recursed into (MAC fails)]

=== Integration and Parallelization
The calculated accelerations for all particles must be integrated over time to update their velocities and positions, thereby advancing the simulation state. Our implementation utilizes the Forward Euler integration method #text(red)[CITATION NEEDED], a straightforward numerical technique. As seen in the `step_time` function within `rust_gdext/src/physics/gravity/controller.rs`, the update proceeds as follows for each particle (body) and its calculated acceleration (acc) over a time step delta:

Update velocity: $v_"new" = v_"old" + a dot Delta t$ \
`rs body.vel += acc * delta;`

Update position: $p_"new" = p_"old" + v_"new" dot Delta t$ \
`rs body.pos += body.vel * delta;` // Update position: pnew​=pold​+vnew​⋅Δt

This specific variant, where the new velocity is used to update the position within the same time step, is sometimes referred to as semi-implicit Euler. While relatively simple to implement, Euler methods can sometimes suffer from stability issues or energy drift in long-term simulations compared to more sophisticated integrators like Leapfrog #text(red)[CITATION NEEDED] or Runge-Kutta #text(red)[CITATION NEEDED] methods, but serve as a functional baseline for this project.

A key advantage of the N-body problem is that the force (and thus acceleration) calculation for each particle is independent of the calculations for other particles within the same time step. This allows for significant parallelization before the integration step. We utilize the rayon #text(red)[CITATION NEEDED] crate to parallelize the main loop in the `NBodyGravityCalculator::step_time` implementation, distributing the calculation of acceleration for each particle across available CPU cores, whether using direct summation for smaller N or the Barnes-Hut algorithm for larger N:

#box[

```rs
// From NBodyGravityCalculator impl in src/physics/gravity/controller.rs
accelerations
    .par_iter_mut()
    .enumerate()
    .for_each(|(i, acc)| {
        // Calculate acceleration for particle `i` using the chosen method
        // (e.g., octree)
         *acc = self.calculate_accel_on_particle(i); // Or similar function call
    });
```

]

This parallelization applies to the computationally intensive force calculation stage, after which the Forward Euler integration updates each particle's state. Combining the parallel force calculation (especially with the $O(N "log"N)$ Barnes-Hut method) with the simple Euler integration allows the simulation of larger systems while managing computational cost.

== Player Controller - ERIK KLAR
The initial player controller was implemented as a simple flying camera, allowing free movement and rotation for exploring the galaxy and star systems. Collision detection and planetary landings were not considered necessary at this stage.

As landing on planets was one of the should have features, the feature began it's development after the marching cubes planets had been implemented. The key sub-features identified for this included: simulating the planet’s gravity on the player, enabling the player to walk along the planet’s surface while rotating accordingly, and allowing the player to jump. With these basic sub-features in mind, development began.

A gravity field around the planets was created using an Area3D node (see #ref(<Area3D>)) along with a spherical collision shape around it. This node allows any physics body that enters it's collision shape to inherit the gravity direction and strength set by the Area3D. The gravity direction is calculated by subtracting the planet's world position with the player's world position. This is updated each physics process to allow the direction to always point towards the planet center.
#figure(
image("images/Planet/Area3D.png", width: 50%),
caption: [Area3D node with collision shape around a planet]
)<Area3D>
To allow for easier exploration, and to simulate the player being in planetary orbit, the player also inherits the planets total velocity as they enter the gravity field. The base speed of the player will then always be greater or equal to the planet's velocity meaning that they don't have to explicitly move to follow the planet's orbit around the sun. The player could also choose to fall towards the planet's gravitational center as they were flying through the gravity field. This confirmed that the gravity was working as intentional. All that was left was to implement landing on planets and the on surface movement.

Rotation was handled by constructing a new target basis for the player, where the 'up' direction was aligned with the inverse of the gravity vector. The player's original basis was then linearly interpolated to the new target rotated basis. This was done to get a smooth rotation. At first this conflicted with the camera movement as the camera tried to rotate the player around it's local y-axis while the movement function rotated the player with the new global basis. This was fixed by simply making sure the camera was rotating the player around the global y-axis instead.

#text(red)[ *TRIVIAL?*
Solving the issue of planetary movement was not too difficult as it could be solved using simple vector mathematics. The resulting velocity on the player could be found by adding the player's velocity with the planet's velocity (see #ref(<PlayerVelocity>)). The gravity is only applied if the player is falling since the rotation mostly keeps the player stuck to the planet. If the player is falling, the gravity vector is simply added to the velocity as described above.
]

#figure(
image("images/Planet/Player Velocity (2).png", width: 50%),
caption: [Total velocity calculation by adding the velocities]
)<PlayerVelocity>

During testing, several issues emerged. The player can occasionally fall off the planets at high velocities, this was solved by lowering the base speed to a small value. A possibility considered was to adapt the base speed depending on planet radius, but was not implemented as it was deemed unnecessary. Another issue is that the player can sometimes start bouncing uncontrollably. A possible explanation to this is the unevenness of the generated terrain. If the player is moving at high speeds they could possibly bounce off of any small bump they encounter. To counteract this, a downward raycast was added beneath the player’s collision shape, supplementing the default collision system. This mostly solves the problem, but further polishing is always possible.

Lastly, the ability to jump was added, which was done by adding a force along the inverse direction of gravity to the player's velocity. One minor issue was observed: if the player entered a second planet's gravity field while still within the first, they could be pulled toward the second planet instead. However, due to the spatial separation between planets, this behavior rarely occurs in practice and was considered a low-priority edge case.

== Planet Generation
This how planet generate :D

=== Height-map planets [Jonatan kanske vill skriva om sina planeter ;) ??]
The initial planets where very simple; using a cube mapped onto a sphere and height maps to create variation in the terrain. This worked fine to start with but the main goal was to create more advanced planets that utilized the marching cubes algorithm and 3D noise in order to get more "interesting looking" terrain, including for instance overhangs and caves. Additionally, a simple planet shader was implemented to add visual interest by coloring the planets based on their height relative to the lowest point.

[BILD]

=== Atmospheres ERIK KLAR
Before starting development on the atmospheres, research was made to try to find existing solutions and inspiration for how it could be done. Two videos that explained two different approaches to the problem were found. One created by Martin Donald that explains a simpler approach #cite(<martin-donald-atmosphere>), and one made by Sebastian Lague that explains a more complex but realistic approach #cite(<sebastian-lague-atmosphere>). The simpler version was implemented at first to quickly get something working, followed by the realistic one, which was the ultimate goal to replicate (see). Both of the solutions tested were implemented as shaders applied to a cube with flipped faces to get the desired effect.

The first iteration of the atmosphere was based purely on ray sphere intersections that could generate a transparent colored sphere around the planet (see #ref(<ray-sphere-atmosphere>)). This was, as mentioned, the simplest version of the atmosphere as it did not account for the sun's position, which meant that it remained a single uniform color regardless of how the sun shone on the planets. Unsuccessful attempts at shading the atmosphere were made by calculating the dot product between each vertex normal and the sun ray directions. The color of the atmosphere were then supposed to change based on the produced value. Due to issues with fetching the correct sun position, and issues with different coordinate spaces, the attempt was unsuccessful. Instead, work on the second version began.

#figure(
image("images/Atmosphere/basic_atmosphere.png", width: 50%),
caption: [Implementation of a basic atmosphere]
)<ray-sphere-atmosphere>
The second iteration intended to generate a more realistic atmosphere that would behave and look like how one might expect it to look like. As mentioned earlier, the inspiration for this atmosphere was found in Sebastian Lague's video, in which he created a planetary atmosphere based on pure Rayleigh scattering. Rayleigh scattering is a physical phenomenon which describes how light is affected by particles much smaller than the wavelength of the light #cite(<RayleighScattering>). This would allow simulating light scattering as it travels through the atmospheres, density fall off, as well as sunsets while standing on planets. The color is based on a three dimensional vector containing three wavelengths that determine the color. Basing the atmosphere color on just Rayleigh scattering meant that something like a red mars-like atmosphere would be impossible to reach "naturally". This approach that was used to solve the issue was to set one of the wavelengths to a much smaller value than the other two. This meant that the color of the shorter wavelength would be much more predominant than the other two. This is due to more of the light scattering at shorter wavelengths, leading to more light rays of thar wavelength hitting the camera. To get reasonable colors, some preset wavelength vectors were made. These were chosen at random as the planets were generated (see #ref(<AtmosphereColors>)).
#figure(
grid(
columns: (auto, auto),
rows: (auto, auto),
gutter: 1em,
column-gutter: -120pt,
[ #image("images/Atmosphere/blue_atmo.png", width: 45%) a) Blue atmosphere #image("images/Atmosphere/purple_atmo.png", width: 45%) c) Purple atmosphere],
[ #image("images/Atmosphere/orange_atmo.png", width: 45%) b) Orange Atmosphere
#image("images/Atmosphere/green_atmo.png", width: 45%) d) Green atmosphere]
),
caption: [Main colors of the different atmosphere variations.]
) <AtmosphereColors>

The chosen implementation for the atmospheres proved to be detrimental to performance. This is due to the slow calculation of light scattering inside the atmosphere. Options to solve this were discussed: such as moving the expensive calculations to a compute shader, implementing differently detailed atmospheres for a LOD system, or keeping the current implementation but include a LOD system to only render the atmospheres fully as the player approaches them. The latter was chosen due to it's simplicity. The system involves dynamically lowering or increasing the scattering point and the optical depth points depending on the player's distance to the planet. At first, the initial scattering and optical depth points were set to 30. This caused the program to run extremely slowly when in close proximity with the planets, but faster when far away. After experimenting with a few other values, setting the numbers to 10 led to a good balance between performance and aesthetics. When comparing the average FPS in a benchmarking scene with one planet, using 30 respectively 10 as the parameters, the results showed a clear difference. Using 30 points for optical depth and scattering gave an average of 39.2 FPS, while using 10 increased the number to 59.9 FPS.

=== Transitioning from height-maps to marching cubes
When transitioning from height-maps to marching cubes, the method of generating the planets needed to change from a cube mapped onto a sphere to a collection of equidistant points in 3D space, with each point containing a noise-value. These points was then used as input to the marching cubes algorithm. The points are constructed as a simple 3D-matrix and @fig:noise-cube shows what these points could look like when visualized using small spheres.

#figure(
image("images/PlanetNoise/noise.png", width: 40%),
caption: [Cube of points]
)<fig:noise-cube>
each dot in noise gets assigned a noise-value..... this is sent to marching cubes

=== Noise planets
The noise planets are the first version of these improved planets and they are formed by generating a 3D matrix of points as shown in the previous section and assigning a random value to all points that is within some radius using a 3D Perlin noise [perlin noise] function. All other points, not within the sphere that is formed by the radius, is to be considered as "empty space" or "above the ground of the planet".

TODO!!!!!!!!!

#grid(
columns: 2,
gutter: 0.75cm,
figure(
image("images/PlanetNoise/noise_planet_sphere.png", width: 160pt),
caption: [Noise planet sphere using marching cubes]
),
figure(
image("images/PlanetNoise/image.png", width: 169.2pt),
caption: [Noise planet with 3D Perlin noise],
),
)

=== Fractional Brownian Motion (fBM) [Flyttas till en annan del tror jag] -- {Kanske skrivas in under fBM planets-rubriken bara?}
Fractional brownian motion[KÄLLA!!OMG!!:D] is a technique utilized in computer graphics that can, among other things, be used for generating realistic-looking terrain[ELLERHÄR KANSKE?!]. The idea is to layer several "octaves" of noise[NOISENOISE], each with increasing frequency and decreasing amplitude, to .......

=== fBM planets
The noise planets did not look great, . To remedy this, research was conducted, and a method called fBM was found and later utilized, to add more complexity and detail to the terrain, making the planets more aesthetically appealing.

Using fBM, as described in section [SECTION fBM], the planets was
Improved planets again using fBM.
As described [ABOVE SOMEWHERE], fBM uses several octaves of noise... etc etc. This was used to create further detail on the planets...... amplitude, frequency, lacunarity, persistence..........

images has interpolation
#grid(
columns: 2,
gutter: 1.75cm,
grid.cell([
#figure(
image("images/PlanetNoise/fbm_planet_1.png", width: 180pt),
caption: [fBM planet],
)<fig:fbm-1>
]),
grid.cell([
#figure(
image("images/PlanetNoise/fbm_planet_2.png", width: 169.4pt),
caption: [fBM planet with flat area],
)<fig:fbm-2>
]),
)

There where some issues with these planets however, as can be seen in the center of @fig:fbm-2, where sometimes the noise function together with the fBM algorithm can cause the boarders of the planet to become part of the planet geometry in the marching cubes algorithm, which can make flat areas on the surface of the planet, or just cut of entire areas............. this happens with varied severity.

Issues: flat areas, visible on fig13

=== Planet generation
In order to generate the planets at run-time, _WE_ initially started by manually testing different values for the FBM parameters until some _decent ranges_ where found and then just randomly assign each parameter a value before creating the data points. Later it was realized that this was not deterministic, meaning that when entering the same solar system (that has the same seed) twice, the planets will not look the same (which we wanted them do to). So to fix this _we_ simply re-used the star seed as explained in section @seed-ref to again use the seed generator with the star-seed for each planet to generate individual seeds for each planet. This makes sure that each planet will be generated deterministically and thus will look the same each time the user enters a specific solar system................... etc

=== Coloring the marching cubes planets ERIK KLAR
The code for coloring the planets was reused from the first planet implementation, with one extension. Cliff edges could be simulated by calculating the dot product between the direction of a specific vertex normal and the direction to the planet center. If the resulting value was close to one, the corresponding fragments at that position were assigned the cliff color (see #ref(<fig:cliff-face>)). Some color themes were created in order to get aesthetically pleasing results (see examples in #ref(<PlanetColors>)).

#figure(
image("images/Planet/cliff_faces.png", width: 50%),
caption: [Cliff faces colored in dark gray]
)<fig:cliff-face>

#figure(
grid(
columns: 3,
column-gutter: -100pt,
[#image("images/Planet/iceWorld.png", width: 50%) a) Ice world],
[#image("images/Planet/earthWorld.png", width: 50%) b) Earth like world],
[#image("images/Planet/lavaWorld.png", width: 50%) c) Lava or Red desert world]
),
caption: [A few select color themes available when generating the planets.]
) <PlanetColors>

Each color theme was then given a specific value in the range [0,1] in order to simulate planet warmth, with 0 being the coldest and 1 being the warmest. These themes were grouped into sets based on their warmth values, which later enabled temperature-based coloring of planets according to their distance from the sun.

To simulate temperature falloff with increasing distance to the stars, the planet warmth was set using this formula:
$
"normalized_warmth"=("planet_warmth" - "min_warmth")/("max_warmth" - "min_warmth")
$
This value was clamped between 0.0 and 1.0 to ensure consistent visual output.

To further randomize appearance and to simulate the impact of atmosphere thickness on surface temperature, a chance that a neighboring theme set could be chosen was also added. For example, if a planet had a warmth of 0.5, there would be a chance that a theme set from a slightly higher or lower warmth could be chosen.

=== Ocean
At this point, it was possible to create some interesting looking planets. But they where still barren and it was time to add some more life to them. ..................

#image("images/Ocean/water_planet1.png")
#image("images/Ocean/water_ball.png")
#image("images/Ocean/water_pinching.png")

Scrolling noise textures and bump mapping..

Scrolling noise for vertex displacement..

Scrolling 3d noise..

Problems: pinching at poles - future fix with e.g. triplanar uv mapping, cube sphere, but maybe OK for now? Also occasianly flickering.. clash with atmosphere?

Depth coloring using screen texture for transparency (multiply the normal color with water color) and depth texture for interpolating color based on depth. Foam color by mixing in white at edges (where depth value is low)..

Using opaque rendering mode so it is included in the depth texture..

Possible improvements for better visuals: beers law, fresnel

== Optimizing the Planet Generation
One of the changes introduced with the transition from height-maps to marching cubes is the increased computational power required to generate a mesh. This is due to the additional dimension involved in the mesh generation process. While height-maps only require a 2D texture to determine the height, as previously mentioned, marching cubes necessitate a 3D array, which significantly increases the computational scale.

=== Compute Shader
The initial optimization step involved transitioning the marching cubes generation process from the CPU to a compute shader.

Unlike the standard rendering pipeline, a compute shader operates independently and is invoked directly by the CPU. In contrast to other shader stages in the rendering pipeline that follow a clearly defined input-output structure, compute shaders utilize an abstract input model that is defined by the user. @openglcs

The primary motivation for employing a compute shader was the significant parallelization capabilities offered by the GPU. As previously noted, the marching cubes algorithm iterates through a grid of points, using the eight neighboring points at each step to construct a polygon. Since each iteration is largely independent of the others, the algorithm is inherently parallelizable— an area in which GPUs excel.

.....

After a series of iterations, a compute shader was successfully implemented to construct a mesh using the marching cubes algorithm. Upon completion, performance testing was conducted to evaluate whether mesh generation via marching cubes was more efficient on the GPU compared to the CPU.

This testing involved feeding identical grids of data points to both the CPU and GPU implementations of the algorithm and measuring the time required to generate the resulting mesh. The tests were performed across a range of different grid sizes to ensure broader applicability of the results.

Contrary to initial expectations, the GPU implementation demonstrated lower performance than its CPU counterpart. It is hypothesized that this outcome is primarily due to the overhead associated with buffer setup and retrieval, which is a known bottleneck in compute shader workflows. Additionally, the triangle buffer was configured to accommodate the worst-case scenario in mesh generation, assuming that each voxel could produce up to five polygons, thereby increasing retrieval time.

Furthermore, unlike Unity—which allows for indirect mesh creation directly within compute shaders to mitigate buffer-related overhead—Godot lacks such functionality. As a result, buffer retrieval must be performed manually, which introduces additional inefficiencies within the engine's pipeline. In the end, this approach did not achieve the intended reduction in planet generation time; however, as alternative methods remain available, the focus will now shift to exploring a different solution.

=== Worker Thread Pooling
An alternative approach involved partitioning the workload across multiple threads. Specifically, in addition to the main thread, a dedicated thread was introduced to handle planet generation requests. Previously, all operations were executed on the main thread, including a loop responsible for generating each planet during solar system creation. This process often led to performance issues, as the computationally intensive planet generation caused noticeable stuttering. By offloading the generation tasks to a separate thread, the main thread could simply dispatch requests and proceed without delay. This adjustment significantly reduced both stuttering and the overall time required for planet generation.

...

The act of planet generation was accomplished through the use of a thread pool. Since creating threads incurs considerable overhead and is relatively resource-intensive, it is desirable to minimize this cost—an objective that thread pools are designed to address. A thread pool functions by allocating a predefined number of threads at startup; in the context of Godot @threadpool2, this initialization occurs during project startup. When a task—such as planet generation—is submitted, it is added to a task queue. One of the pre-allocated threads (commonly referred to as workers) retrieves the task from the queue, executes it, and then proceeds to the next available task. This approach eliminates the need to create new threads for each operation, thereby improving efficiency and performance. @threadpool1

=== Chunking / Octree

== Surface Elements
In order to enhance the variety of the planet's surface details, it was decided to incorporate elements such as grass, bushes, trees, and oceans. This addition aims to increase the diversity of the planet's landscape.

The implementation structure for surface details is primarily based on the work presented in the blog _Population of a Large-Scale Terrain_@surfacedetails1, in which the author classifies surface elements into two distinct categories: _details_ and _features_.

The _details_ category includes elements such as grass, which are rendered in close proximity to the player. Precise positioning of these elements is not critical, as minor discrepancies typically go unnoticed due to their abundance and the player's limited focus on individual instances (e.g., the position of a single blade of grass).

In contrast, the _features_ category encompasses elements like trees. These are intended to be visible from a distance and require consistent placement, as irregularities in their positions are more easily perceived and can negatively impact the visual coherence of the scene.

For both categories, it remains important to ensure that the generated elements are aligned with the normal of the underlying mesh. However, due to their differing visual and functional requirements, each category necessitates a distinct implementation approach.

=== Surface Details (Grass)
To generate surface details, the system first identifies the current chunk associated with the player, along with adjacent chunks within a defined range. It then iterates through all triangles in the mesh data of each relevant chunk. For each triangle, a random point is generated within its bounds using barycentric coordinates, as described in @barycentriccoordiantes1.

Given a triangle with vertices $bold(a)$, $bold(b)$, and $bold(c)$, and two random variables $r_1$ and $r_2$, a random point $bold(d)$ within the triangle can be computed using the following formula @barycentriccoordiantes1:
$ bold(d) = (1 - sqrt(r_1))bold(a) + sqrt(r_1)(1 - r_2) bold(b) + sqrt(r_1) r_2 bold(c) $
This method ensures that the point $bold(d)$ lies uniformly within the triangle. To determine the orientation of the surface detail, the normal vector of the triangle is calculated via the cross product of its edge vectors:
$ bold(n) = bold(b) - bold(a) crossmark bold(c) - bold(a) $
The resulting normal vector $bold(n)$ is then normalized. An orthonormal basis is constructed using this normal as the y-axis. The x-axis vector is derived using the following logic:

```cs
        Vector3 upVector = normal;
				Vector3 xVector;
				if (Mathf.Abs(upVector.Y) < 0.99f)
					xVector = new Vector3(0, 1, 0).Cross(upVector).Normalized();
				else
					xVector = new Vector3(1, 0, 0).Cross(upVector).Normalized();
```

This approach selects an appropriate reference vector (either $(0, 1, 0)$ or $(1, 0, 0)$) based on the vertical component of the normal, ensuring numerical stability in the computation. The z-axis vector is then obtained by computing the cross product of the x and y vectors. This yields a complete, orthonormal basis aligned to the triangle, which is subsequently used to orient the surface detail accordingly.

To evaluate this approach, a simple plane mesh was utilized, yielding the following result:
#grid(
columns: 2,
gutter: 0.75cm,
grid.cell([
#figure(
image("images/Grass/grass1.png", width: 200pt),
caption: [Simple Grass Spawning]
)<fig:simplegrass1>
]),
grid.cell([
#figure(
image("images/Grass/grass2.png", width: 200pt),
caption: [Wire-frame of Simple Grass Spawning]
)<fig:simplegrass2>
])
)
An initial issue encountered was that only a single surface detail was rendered per face (see @fig:simplegrass2), making the total number of rendered instances dependent on the face count of the mesh.

To address this, a density parameter was introduced to control the desired number of instances. In order to distribute the instances proportionally across the mesh, the total mesh area is first computed. Then, for each face, the ratio of its area to the total area is calculated and used to determine the number of instances on that face, as demonstrated in the following code snippet:

```cs
			float faceAreaRatio = faceArea / totalArea;
			int faceInstances = Mathf.FloorToInt(density * faceAreaRatio);
```

By iterating through each face and applying this calculation, the number of instances is determined and their positions and orientations are computed accordingly. With the density parameter set to 100, the resulting distribution is shown in @fig:simplegrass3 (with density set at 100).
#grid(
columns: 2,
gutter: 0.75cm,
grid.cell([
#figure(
image("images/Grass/grass3.png", width: 200pt),
caption: [Density-Based Grass Spawning]
)<fig:simplegrass3>
]),
grid.cell([
#figure(
image("images/Grass/grass4.png", width: 200pt),
caption: [Wire-frame of Density-Based Grass Spawning]
)<fig:simplegrass4>
])
)
One issue with this simple implementation is that when faces are very small in comparison to others it will not render any instances as the calculation above will yield a instance number lower than 1 which means that no surface details will be added on that face, for example you can see it on a basic sphere mesh (see @fig:simplegrass5 and @fig:simplegrass6), however for our generated mesh the difference in the faces will not be as extreme as in @fig:simplegrass5 so it suffices for the moment.
One limitation of this method is that faces with relatively small areas may yield fewer than one instance, resulting in no surface detail being rendered on those faces. This effect is illustrated in @fig:simplegrass5 and @fig:simplegrass6 using a basic sphere mesh. However, for the generated mesh in this project, face size variation is significantly less pronounced than in @fig:simplegrass3, making this limitation acceptable for the current purposes.
#grid(
columns: 2,
gutter: 0.75cm,
grid.cell([
#figure(
image("images/Grass/grasss5.png", width: 200pt),
caption: [Density-Based Grass Spawning on a sphere]
)<fig:simplegrass5>
]),
grid.cell([
#figure(
image("images/Grass/grass6.png", width: 200pt),
caption: [Wire-frame of Density-Based Grass Spawning on a sphere]
)<fig:simplegrass6>
])
)
With this issue addressed, the development of a grass shader was initiated.

... The grass was based on https://gdcvault.com/play/1027214/Advanced-Graphics-Summit-Procedural-Grass @grass1 and https://youtu.be/bp7REZBV4P4?si=KR9xS2I04L58P0NT @grass2

And in the end the result for surface details such as grass on a planet looked like:
#figure(
image("images/Grass/grass7.png", width: 300pt),
caption: [Grass on a planet]
)

blablabl GPU instancing (multimesh) (borde nog vara under surface elements delen)
=== Surface Features (Trees)
blabla
For figuring out the tree position I first used bilinear interpolation between a plane with the corners (v1, v2, v3, v4) and picked a random point. Later switched to possion-disc sampling. https://www.cs.ubc.ca/~rbridson/docs/bridson-siggraph07-poissondisk.pdf

Unlike the method utilized for Surface Detail, Surface Features must remain visible at greater viewing distances. Accordingly, features are not limited to rendering within high-resolution chunks proximal to the camera. Additionally, the technique previously employed for Surface Detail—relying on dense mesh triangle data—is incompatible with the current octree-based (_Hopefully_) terrain representation. The hierarchical level-of-detail inherent in octree chunking results in a significantly reduced triangle count at lower resolutions, rendering geometry-dependent techniques ineffective.

To address this limitation, an alternative approach based on ray-casting was implemented. Rays are cast inward from randomly sampled points on the surface of the planet’s axis-aligned bounding box (AABB). Upon intersection with the terrain surface, the hit position and surface normal are extracted. These data points serve as the basis for procedurally instantiating surface features, following a similar construction pipeline to that used in the Surface Detail system, but decoupled from mesh density constraints.

For the initial generation of points on the axis-aligned bounding box (AABB), a uniform random sampling strategy was employed. Points were computed via bilinear interpolation across each face of the AABB using the following logic:

```cs
				// Get a random point in the face
				float u = (float)GD.RandRange(0.0, 1.0);
				float v = (float)GD.RandRange(0.0, 1.0);
				// Bilinear interpolation
				Vector3 randomPoint = v1 + u * (v2 - v1) + v * (v4 - v1);
```

Here $v_1$, $v_2$, $v_3$, $v_4$ represent the four vertices defining a face of the AABB. This approach assumes that each face is a perfect rectangle, which holds true in the case of AABBs. For non-rectangular surfaces, a more generalized interpolation method would be required.

Although this uniform sampling technique is computationally efficient and straightforward to implement, it tends to produce uneven spatial distributions. Specifically, it may result in clustering of points in some regions and sparse coverage in others—an outcome that is undesirable when attempting to ensure consistent surface feature placement across the planetary surface.(_Infoga Bild på skillandeden._)

To achieve a more spatially uniform distribution of points, the sampling method was subsequently replaced with Poisson-disc sampling. Unlike uniform sampling, Poisson-disc sampling ensures that each point is separated by a minimum distance $r$ @bridson2007fast, thereby avoiding clustering and producing a more even distribution of samples.
The algorithm used follows the method described in _Fast Poisson Disk Sampling in Arbitrary Dimensions_, which operates as follows @bridson2007fast:

_Step 1. _ Initialize a background grid with cell size $frac(r, sqrt(n))$ where $n$ is the dimensionality of the space (in this case, $n = 3$).

_Step 2. _ Randomly select an initial sample within the domain and insert it into both the active list and the background grid.

_Step 3. _ While the active list is non-empty:
#list(indent: 2cm,
spacing: 1em,
[Randomly choose a sample from the active list.],
[Generate up to $k$ candidate points within an annulus of radius $r$ to $2r$ around the chosen sample.],
[For each candidate:

- Reject it if it lies outside the domain or is closer than $r$ to any existing sample (checked efficiently using the grid).
- If it is valid, add it to both the active list and the grid.],
  [If none of the $k$ candidates are accepted, remove the sample from the active list.])

This revised sampling technique yielded a significantly improved distribution of surface feature placement, resulting in a more visually and functionally satisfactory outcome.

Because we want to see a different variations of features such as large tree, small trees and rocks for example we need a way to distribute the features. To do that we use the Alias method @aliasvose. The Alias method works by taking a discrete lists of probabilities and returns a value $1 <= i <= n$ according to that list. For example if we have...

=== Oceans
blabla
=== Biomes (Kanske)

== System Generation
System generation means creating a system from a seed such that the system is stable and has certain aesthetics. The generation includes placement of planets and moons such that they are in a stable orbit. It's also responsible for creating the seeds of the individual bodies in the system.

The generation of systems has to make a tradeoff between realism and aesthetics, while keeping the system stable. Creating stable system with realistic distance/planet-size is easy but comes at a cost of aesthetics. With distances that large the planets become really hard to see, which doesn't make for a good experience.

Creating smaller distances and larger planets helps with aesthetics but can come at a cost of stability, specifically for moons. Having larger planets means that the moons will have to be orbit further. This increases the risk of instability due to the moons interacting with other planets.

=== General Flow
The input seed gets added as the seed to a random number generator. This ensures that it produces the same random numbers each time the system gets generated. For each attribute of the system a value for that attribute gets randomized within an interval.

```ts
func generateSystemDataFromSeed(s: int):
	var r = RandomNumberGenerator.new()
	r.seed = s
```

=== Star
The only randomized attribute of the star is it's color. Which gets picked randomly out of a list. The mass of star is constant across all systems.

```py
  var colors := [
		Color(1, 0.14, 0), # Red (Red dwarf or red giant)
		Color(1, 0.5, 0), # Orange (Orange dwarf)
		Color(1, 1, 1), # White (White star)
		Color(0.5, 0.5, 1), # Light blue (A-type star)
		Color(0.2, 0.2, 1), # Blue (Hot B-type star)
		Color(0.1, 0.1, 1), # Very hot blue (O-type star)
		Color(0.8, 0.8, 1), # Pale blue-white (F-type star)
		Color(0.9, 0.8, 0), # Yellow-orange (K-type star)
		Color(0.8, 0.6, 0.4) # Yellow-brownish (G-type star, slightly more red)
	]
```

=== Planets
The system randomizes how many planets it has.

```py
  @export var MIN_NUMBER_OF_PLANETS: int = 3
  @export var MAX_NUMBER_OF_PLANETS: int = 8
```

The planets are placed at set distances away from their star.

```py
  @export var DISTANCE_BETWEEN_PLANETS: float = 200.0;
  @export var BASE_DISTANCE_FROM_SUN: float = 300.0;
```

In order for a planet to stay at the same distance from its star, it gets an initial velocity perpendicular to the vector pointing towards the star. The velocity scales with the stars mass $m$, the gravitational constant $G$ and inversely with the orbit radius $r$.

$ sqrt(G dot m/ r) $

For planets we randomize
#list( [Mass], [Radius])
= Result

= Discussion
== Societal and Ethical aspects ERIK KLAR
During the planning phase of this project, two primary ethical and societal concerns regarding the use of procedural content generation (PCG) in game development were identified. These concerns were: (1) the potential displacement of game designers by PCG algorithms, and (2) the risk of generating content that lacks sufficient variety, leading to a repetitive player experience.

The first point is related to the concern about game designers losing their jobs to procedural generation algorithms. However, the development process in this project revealed that significant manual effort remained necessary. Selecting appropriate color themes for planets, scaling and positioning the celestial bodies in a plausible manner, as well as to generate aesthetically pleasing terrain, all required substantial manual tweaking of parameters and settings. Moreover, PCG algorithms have the potential to generate content that may be faulty, unrealistic or unnatural. Solutions to these issues include setting specific constraints before executing the algorithms, or involving a human designer at the end of the content generation #cite(<computers13110304>). Although it is still a possibility that future PCG algorithms can automate these processes, current implementations seem to depend on a collaborative relationship between human designers and algorithmic systems.

The second concern focused on the potential for PCG to produce content that is overly repetitive, thereby diminishing the quality of the user experience. While the primary objective of this project was not to create an engaging gameplay experience, certain measures were nonetheless taken to mitigate repetitiveness. For instance, planetary coloration was randomized based on each planet’s distance from the sun, with additional randomization applied to simulate variations in atmospheric thickness. These methods introduced greater diversity in the generated content, demonstrating that careful parameterization and randomness can effectively counteract some of the inherent risks associated with procedural generation.

== Process Discussion
== Future work

= Conclusion

//
// Andra får gärna läsa och se om ni håller med---
//
== Project purpose

The projects purpose underwent a greater change after the feedback from the planning report. The original purpose is what follows:

// Ett fint block, idk om vi ska ha det
// mindre teckenstorlek, indraget? I think
#block(
fill: luma(230),
inset: 8pt,
radius: 4pt,
)[
"The aim of this project is to simulate solar systems through procedurally generated planets, utilizing computer-generated noise such as Perlin noise, together with the marching cubes algorithm. The composition of the solar systems can vary – from a sun with a single planet to more complex systems with multiple planets and additional celestial bodies such as moons. To mimic the natural movements of these celestial bodies, a simplified physics simulation will be implemented.

This project also aims to explore and combine different techniques for optimization to ensure that the simulation will run in a performance-efficient manner."
// Behövdes källa för vårt eget??na?
]

It appeared that it was unclear to what the project set out to do, which after internal discussions the project team agreed upon. Any changes, particularly to the purpose, sought to address the following three problems:

1. A great deal had been explained specifically about solar systems in the planning report, while the team, in reality, had drifted towards wanting to create an entire galaxy of solar systems instead.
2. An entire section of the planning report was dedicated to performance and optimization, as well as a part of the purpose. This played a part in making it unclear whether this projects major focus was about optimization, or something else.
3. It was unclear how this project differs from a similar bachelor's thesis project called Exo Explorer, from a couple of years ago.
   // källa för exo Explorer?

Through internal discussions, consultation with our supervisor, and study of the previously mentioned feedback, a rewrite to facilitate a clarification of project goals and a refinement of its scope, took place. The resulting purpose can be seen in @purpose-ref.

With the aforementioned three problems, it also entailed a change of...
moscow?, since it suffered from similar problems as the planning report itself...--..-.--
blabla--??
?

== Performance
// Discussion?
Initially in the planning report, the goal of this project was to achieve an average of 60 FPS in any part of the simulation. However, after closer research we opted to focus on consistent frame times instead, as detailed in... method

///
#pagebreak()

#heading(numbering: none)[Glossary]

- _Mesh:_ A collection of vertices, edges, and faces that define the shape of a 3D object.
  - _Vertex:_ A point in 3D space, representing a corner of a triangle. Stores attributes like position (x, y, z), normal, UVs, and optionally color.
    - _Normal:_ A vector that is perpendicular to a surface. Used in lighting calculations.
    - _UV coordinates:_ a 2D coordinate system used to map a texture image to a 3d object surface.
  - _Edge:_ A line segment connecting two vertices.
  - _Face (Triangle):_ A flat surface bounded by edges (typically a triangle in real-time graphics). Triangles are used because they are always planar.
- _Rendering Pipeline:_ The sequence of stages a GPU uses to render a 3D scene.

- _Shader:_ A small program that runs on the GPU. Primarily used for controlling how objects are rendered.

  - _Vertex Shader:_ Processes each vertex; transforms positions from model space to clip space and manipulates vertex attributes.
    - _Model Space:_ The object's local coordinate system.
    - _Clip Space:_ A standardized coordinate system used by the GPU after vertex transformation.
  - _Fragment Shader (Pixel Shader):_ Determines the final color of each fragment, often using lighting and textures.
    - _Fragment:_ A "potential pixel" generated during rasterization.
    - _Rasterization:_ The process of converting vector graphics (triangles) into raster graphics (pixels).
  - _Compute Shader:_ Used for general-purpose computation on the GPU (not directly part of the rendering pipeline).

- _Procedural Generation:_ The creation of data (models, textures, etc.) algorithmically, rather than manually. This often uses noise functions and other mathematical techniques to create varied and complex results.

  - _Noise Function (Perlin, Simplex):_ Algorithms for generating pseudo-random values with a smooth, continuous appearance, used for procedural generation.
  - _Marching Cubes:_ An algorithm to create a triangle mesh from a 3D scalar field (e.g., noise data).

- _LOD (Level of Detail):_ Rendering objects with varying complexity based on distance.
- _PCG_: Procedural content generation

#pagebreak()

#bibliography("src.bib")

#pagebreak()

#v(250pt)
= Kladd

== Fundamentals of 3D Graphics for Procedural Planet Generation

This section outlines how we utilize the GPU and the _rendering pipeline_ for our procedural planet generation project.

=== GPU Architecture and the Rendering Pipeline

Modern GPUs are _massively parallel processors_. Unlike CPUs, which typically have a few very fast and versatile cores, GPUs contain _many_ cores optimized for performing the same operation on many data points simultaneously. This makes them ideally suited for the numerous independent calculations involved in 3D rendering. We will make extensive use of the GPU's _rendering pipeline_, specifically leveraging _vertex shaders_, _rasterization_, _fragment shaders_, and output merging to transform our 3D planet data into a 2D image.

We will optimize our rendering process to leverage the GPU's parallel capabilities, achieving _real-time_ performance.

=== Meshes: Representing 3D Objects

Our planets are represented as _meshes_, collections of interconnected triangles. Each _vertex_ in a mesh stores its position, a _normal_ vector, and _UV coordinates_.

=== Key Concepts and Considerations

- _Real-time Rendering:_ We require real-time rendering for an interactive experience, maintaining a smooth frame rate.

- _Performance Optimization:_ We'll employ various optimization techniques:

  - _LOD (Level of Detail):_ Using different mesh complexities based on distance.
  - _Occlusion Culling:_ Avoiding rendering hidden objects.
  - _Compute Shaders:_ Potentially using _compute shaders_ to accelerate terrain generation or physics calculations.

- _Procedural Generation:_ We'll use _noise functions_ and the _marching cubes_ algorithm to generate varied and realistic planet surfaces.

=== Relevance to the Project

- _Efficient Terrain Generation:_ Optimizing the mesh generation process is essential for performance.
- _Realistic Visuals:_ _Fragment shaders_ are crucial for lighting, texturing, and atmospheric effects.
- _Interactive Exploration:_ Maintaining a high frame rate requires careful optimization of the entire _rendering pipeline_.

== Anton saker

- Fractional brownian motion (perlin noise flera stacked lixom)
  https://iquilezles.org/articles/warp/
  https://iquilezles.org/articles/fbm/
  https://thebookofshaders.com/13/

Marching cubes as described by William E. Lorensen and Harvey E. Cline (1987) #text(red)[LÄNK https://dl.acm.org/doi/10.1145/37401.37422].. is an algorithm for constructing 3D surfaces. Though originally designed for the medical industry, it is often used today in (for instance) game development, in combination with different types of noise to create the worlds which players can explore.

NOISE:

https://dl.acm.org/doi/pdf/10.1145/325334.325247 - Ken Perlin (1985)

https://mrl.cs.nyu.edu/~perlin/noise/ - Ken Perlin (2002)

https://mrl.cs.nyu.edu/~perlin/paper445.pdf - Ken Perlin (2002)

Texturing & Modeling A Procedural Approach Second Edition (en bok) - 1998 - David S. Ebert, F. Kenton Musgrave,
Darwyn Peachey, Ken Perlin, Steven Worley ?????

#text(red)[ska tas bort sen:

The planets should be constructed using noise and given shape using the marching cubes algorithm.

- How should the planet generation be accomplished?
- How could the planets be further distinguished from each other, apart from using noise?
- In what way should the solar system be generated procedurally?
-

How do we create a planet?

NOISE:
https://dl.acm.org/doi/pdf/10.1145/325334.325247 - Ken Perlin (1985)

https://mrl.cs.nyu.edu/~perlin/noise/ - Ken Perlin (2002)

https://mrl.cs.nyu.edu/~perlin/paper445.pdf - Ken Perlin (2002)

Texturing & Modeling A Procedural Approach Second Edition (en bok) - 1998 - David S. Ebert, F. Kenton Musgrave,
Darwyn Peachey, Ken Perlin, Steven Worley ?????

MARCHING CUBES:

Marching cubes as described by William E. Lorensen and Harvey E. Cline (1987) #text(red)[LÄNK https://dl.acm.org/doi/10.1145/37401.37422].. is an algorithm for constructing 3D surfaces. Though originally designed for the medical industry, it is often used today in (for instance) game development, in combination with different types of noise to create the worlds which players can explore.
]

Det vi kom på:

- Kunna genera en planet
  - Generera planeter med olika utseende
    - noise
    - biomes
      - med hjälp av noise
  - Generera solsystem med planeter och omloppsbanor
  - Andra himlakroppar är i en omloppsbana runt planeter
  - Kunna åka mellan solsystem som genereras "on the fly"

=== Task gpt från planeringsrapport

#text(maroon, lang: "se")[
GPT idé:

- Terränggenerering och Rendering

  - Delmål: Utveckla en fungerande algoritm för terränggenerering baserad på brus (t.ex. Perlin-brus) och implementera marching cubes för att skapa en realistisk 3D-modell av planetens yta.
  - Delproblem: Hantering av överhäng, fluktuerande topografi och detaljeringsnivåer.

- Kamerakontroll och Utforskning

  - Delmål: Skapa en användarvänlig kamerakontroller som möjliggör utforskning av den genererade terrängen.
  - Delproblem: Balansen mellan att ge en fri utforskningsupplevelse och att begränsa kamerarörelser så att prestandan inte äventyras.

- Simulering av Fysik

  - Delmål: Implementera en förenklad fysikmotor som kan hantera planeternas omloppsbanor kring solen samt andra himlakroppars rörelser.
  - Delproblem: Beräkna gravitationseffekter och säkerställa att systemet är stabilt över tid.

- Prestandaoptimeringstekniker

  - Delmål: Utforska och implementera olika tekniker för att optimera prestanda, såsom LOD, occlusion culling och användning av spatiala datastrukturer.
  - Delproblem: Jämföra de olika teknikerna och utvärdera vilka som ger bäst prestanda utan att kompromissa med visuella detaljer.
    ]

== Optimization

The aim is to construct a real-time application, and thus, optimization techniques are increasingly important. Inefficient algorithms and resource management will eventually lead to performance bottlenecks. To address this, proven techniques and smart solutions will be explored throughout all parts of the project to improve overall efficiency.

// https://books.google.se/books?hl=sv&lr=&id=uiz6IKAVxP8C&oi=fnd&pg=PP1&dq=Level+of+detail&ots=lQuMhPSBR8&sig=eHkYBUe-rSUVR-9I-z35qCslebQ&redir_esc=y#v=onepage&q&f=false

- Level of Detail (LOD)
  A common optimization technique that reduces the complexity of rendered objects based on their distance from the viewer. This technique balances visual fidelity and performance by making use of the effects of perspective — objects that move further away appear smaller on the screen and contribute less to the final picture. By decreasing the complexity of these distant objects, the visual difference is often unnoticeable or insignificant, or at least, a necessary tradeoff to maintain performance efficiency.

There exist different ways to implement LOD. - Discrete LOD
The traditional approach to LOD. It involves creating multiple versions of each object at different levels of detail before running the program. At runtime the appropriate version is selected depending on the distance from the viewer. Whilst efficient and simple to implement, the discrete steps between the versions could be noticeable at runtime and you would also need to construct those versions in the first place. Due to this it might prove inadequate for the purposes of this project, since the solar system, its planets, and its contents, are procedurally generated each time the program is run. This leads us to: - Continuous LOD
Works similarly to Discrete LOD but now it functions by instead constructing the LODs at runtime. In regards to the terrain generation and the marching cubes algorithm, it's likely that Continuous LOD would be implemented in this project by dynamically decreasing the resolution of marching cubes depending on the distance to the viewer.

- Chunks
  Commonly involves splitting the environment into a square grid. It's not an optimization technique in and of itself, although with chunks implemented it opens up for other techniques to make use of it.

Pausar/tar bort chunks som inte syns & Multithreading v, typ

- Parallelization (Multithreading)
  abc

- Compute Shaders
  abc

- Culling
  Culling within computer graphics involves removing objects that do not need to be rendered since they are not visible. Multiple techniques for this exist, some of those are:
  - View-frustum culling // https://docs.godotengine.org/en/stable/tutorials/performance/optimizing_3d_performance.html#culling
    Objects outside of the camera's viewing frustum (the visible volume of the scene) are culled to prevent them from rendering unnecessarily, which improves performance. Godot appears to handle this automatically.

  - Occlusion culling // https://docs.godotengine.org/en/stable/tutorials/3d/occlusion_culling.html#doc-occlusion-culling
    Objects that are hidden behind other objects are culled, since they are not visible to the camera. Godot appears to have settings to enable this.

  - Detail culling // källa?
    Objects that are at a certain distance away from the camera are culled, since they occupy a very small area of the picture.

== Procedural Generation
Procedural generation is described as “_the algorithmic creation of game content with limited or indirect user input_” @proceduralgame:2016. To further elaborate, it allows computers to algorithmically produce data/game content, such as textures, terrain structures, or even whole worlds without needing to be handcrafted by a human.
In #text(yellow)[our] case #text(red)[we] would use procedural generation to create terrain that forms planets and in turn create solar systems out of it.

== Noise
As mentioned in the previous section, procedural generation algorithms work by taking data input and producing generated content out of that, which in #text(yellow)[our] case would be terrain.
However that arises is how #text(red)[we] get these data points. Instead of manually getting them #text(red)[we] can instead use something called noise functions to get these data points. \
In @noisefunctions2:2010 noise functions are described as “_the random number generator of computer graphics_”. There exists different kinds of noise functions which generate random noise in different ways and for procedural terrain generation the relevant noise functions as mentioned by @noisefunctions1:2016 includes: diamond-square algorithm, value noise, perlin noise, simplex noise, and Worley Noise. What will probably be most present in this project however is perlin noise.

== Terrain Generation
There are several different kinds of techniques that can achieve procedurally generating terrain and #text(red)[we] decide now in the beginning to make use of heightmaps and the marching cubes algorithm as these techniques allow us to generate terrain in real time.

=== Height maps
Height maps are used as a data structure that stores the elevation for each vertex in a mesh’s geometry.@heightmaps:2019 In most cases height maps are stored as image files, where each color of the pixel in the image represents the height of the corresponding vertex in the mesh, this is usually stored as a gray scale image where the brightness instead is used as the elevation. However there are downsides to this approach as a height map can only store a single elevation and thus cannot have complex terrain, such as overhangs.

=== Marching Cubes
The Marching cubes algorithm works in general by locating the user-specific values and creating triangles out of it. In more detail #text(red)[we] first create a logical cube with eight points in each corner of this cube. The algorithm then determines how the surface intersects this cube and then moves to the next cube present in the input data.
To find if the surface intersects this cube #text(red)[we] look at each corner of the cube which corresponds to some input data and check if the value of the data is higher (or equal) to the threshold of the surface #text(red)[we] are constructing. These vertices are described as being inside (or on) the surface @marchingcubes:1998. Since there are eight vertices in each cube and only two states, inside and outside, there are only $2^8 = 256$ ways a surface can intersect the cube. Thus #text(red)[we] use a lookup table to find out the triangulation of the vertices and construct triangle meshes out of these, and #text(red)[we] loop through all of the input data to construct the whole mesh.

== metod

#text(maroon, size: 12pt)[
???

Our project will rely heavily on real-time 3D graphics techniques. #text(red)[We] will leverage the GPU's massively parallel architecture and its rendering pipeline, including vertex and fragment shaders, to efficiently render procedurally generated planets. Key optimization strategies will include level of detail (LOD), occlusion culling, and potentially compute shaders. #text(red)[We] will represent planets as triangle meshes, utilizing procedural generation techniques based on noise functions and the marching cubes algorithm. A glossary of key 3D graphics terms is provided in Appendix X...
]

== ...
...

#text(red)[ Kanske inte behövs
=== Light
Simulating illumination in the systems is important for player experience, not only because it's desirable when things look "better". But also because illumination is often an important differentiator between planets. (Different colored atmosphere, no atmosphere, gas giants etc.)

Simulating light correctly can be difficult but it is helpful that, in general, detail is only needed once the player is close to a planet. Which would mean that the player is far away from other planets, opening the possibility for big performance gains due to only one having to render one planet in detail at a time.]

== Benchmarking system

Benchmarksystem? passande under 'metod' också?

...With the implementation of all 'must haves' of the moscow? the mvp.
everything connected with the galaxy map provided a base implementation of the core systems of this project further on work would continue on these, improving performance, changing methodologies, new features should/could haves...- To document improvements and declines in performance, a streamlined benchmarking system was implemented to provide an additional point of argument.-..-

https://www.tomshardware.com/news/what-makes-a-good-game-benchmark
Discusses benchmarking practices.
Repetability is important. Mentions that AC Odyssey was notoriously inconsistent to benchmark due to random weather effects such as rain and cloud coverage having a great impact om the benchmark results. Goes well with the deterministic nature of this project.
Having the benchmark represent actual situations that the user could encounter, -- mentions is a good thing as well. An early version of the galaxy map benchmark ran at a fact pace, moving the player at a speed that won't be encountered in a typical user scenario. The speed was decreased. // kanske-.-...

// ...
=== General benchmarking system

benchmarking system. fps, frame times, memory usage. runs specific scenes and tracks these.

e.g. a scene of the galaxy map (@galaxy-map-ref), testing the implications of generating new chunks of stars (@star-chunk-img) continuously, for a duration of time.

// typ? idk
=== Other benchmarking

Other, more specific, benchmarking that differs between separate features.

e.g. measure of milliseconds for marching cubes CPU vs GPU computations.

#show: appendix

#pagebreak()
= \ Pseudocode for Barnes-Hut algorithm <bh_pseudo>
#algorithm({
import algorithmic: *
State[*const* $theta$]
State[*const\* $epsilon$]
State[]

Function("Barnes-Hut-Gravity", args: ("G", "particles"), {
Cmt[Get the number of particles]
Assign[$n$]#FnI[length][particles]]

    State[]
    Cmt[If no particles, return empty acceleration array]
    If(cond: $n == 0$, {
      Return[#FnI[array][#FnI[Vec3][0, 0, 0]\; 0]]
    })

    State[]
    Cmt[\1. Build the Octree]
    Assign[octree_root][#FnI[buildOctree][particles]]

    State[]
    Cmt[\2. For each particle, calculate acceleration using the octree]
    Cmt[Initialize acceleration array]
    Assign[acc][#FnI[array][#FnI[vector][0, 0, 0]\; $n$]]

    State[]
    For(cond: [$i$ *in* $0..n$], {
      Cmt[Get the target particle's position]
      Assign[$"pos"_i$][particles[i].*position*]
      Assign[acc[i]][#CallI[recurse][G, octree_root, particles, $i$, $"pos"_i$]]
    })

    State[]
    Return[acc]

})
})

#algorithm({
import algorithmic: \*

Function("recurse", args: ("G", "node", "all_particles", $i$, $"pos"_i$), {
Cmt[Get node properties (mass, center of mass, size)]
Assign[$m_n$][node.*mass*]
Assign[$"CoM"_n$][node.*CoM*]
Assign[$s_n$]#FnI[width][node]]

    State[]
    Cmt[Calculate vector from target to node's center of mass]
    Assign[$Delta"pos"$][$"CoM"_n$ - $"pos"_i$]
    Assign[$d$][|$Delta"pos"$|]

    State[]
    Cmt[Check the _Multipole Acceptance Criterion (MAC)_]
    If(cond: $s_n / d < theta$, {
      Cmt[Node is far enough, use approximation]
      Assign[accel][#CallI[calcAcc][G, $m_n$, $Delta"pos"$]]
      Return[accel]
    }, {

      State[]
      Cmt[Node is too close]
      If(cond: FnI[isLeaf][node], {
        Cmt[Leaf node, calculate direct interactions with particles in this node]
        Assign[$a_"tot"$][#FnI[vector][0, 0, 0]]
        For(cond: [$j$ *in* node.*body_range*], {
          If(cond: $j != i$, {
            Assign[$"pos"_j$][all_particles[j].*position*]
            Assign[$m_j$][all_particles[j].*mass*]
            Assign[$Delta"pos"_"direct"$][$"pos"_j$ - $"pos"_i$]

            State[]
            Assign[$a_"direct"$][#CallI[calcAcc][G, $m_j$, $Delta"pos"_"direct"$]]
            Assign[$a_"tot"$][$a_"tot"$ $+ a_"direct"$]
          })
        })
        Return[$a_"tot"$]
      })

      Else({
        Cmt[Internal node, recursively call for children]
        Assign[$a_"tot"$][#FnI[vector][0, 0, 0]]
        For(cond: [child *in* node.*children*], {
          Assign[$a_"child"$][#FnI[calculateAcceleration][G, child, all_particles, $i$, $"pos"_i$]]
          Assign[$a_"tot"$][$a_"tot"$ $+ a_"child"$]
        })
        Return[$a_"tot"$]
      })
    })

})
})

#algorithm({
import algorithmic: \*

Function("calcAcc", args: ($G$, $m$, $r$), {
Assign[$d_"soft"$][$sqrt(r^2 + epsilon^2)$]
Assign[$a$][$G * m / d_"soft"^3 * r$]
Return[a]
})
})
