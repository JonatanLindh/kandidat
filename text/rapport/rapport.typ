#import "@preview/wordometer:0.1.4": word-count, total-words
#import "@preview/numbly:0.1.0": numbly
#import "@preview/algorithmic:0.1.0"
#import algorithmic: algorithm
#import "@preview/timeliney:0.2.0"

#show figure.where(
  kind: table
): set figure.caption(position: top)

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
      
      #image("ozempic.png", width: 40%)


      Exo Explorer 2? Chalmers Galaxy Solar-System Planet Generation App (CGSSPGA)
      
      #set text(size: 17pt)
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

#set page(numbering: "i")
#counter(page).update(1)

#pagebreak()

#heading(numbering: none, outlined: false, bookmarked: true)[
  Abstract #text(red)[JONATAN KLAR]
]
Procedural Content Generation (PCG) offers powerful methods for creating vast virtual universes, particularly relevant for space exploration simulations, yet poses significant challenges regarding computational scale, performance, and plausibility. This project addresses these challenges by developing and simulating a physics-based, procedurally generated, and explorable galaxy within the Godot game engine. The core objective was to create a deterministic and computationally efficient model. Planets were generated using noise functions (Perlin, fBm) combined with the Marching Cubes algorithm to create complex terrains, enhanced with procedurally placed oceans, vegetation, and physically-based atmospheres simulating Rayleigh scattering. Optimization was crucial, employing techniques like multi-threading for planet generation, chunking, and Level of Detail (LOD) managed via octrees. A custom N-body physics engine, featuring a parallelized Barnes-Hut algorithm ($O(N "log"N)$) built upon a Morton-code-based linear octree (implemented in Rust via GDExtension), simulates celestial mechanics. Solar systems and the galaxy structure utilize seeded randomization for determinism and reproducibility. Exploration spans multiple scales, from a galaxy map interface down to planetary surface navigation with a physics-aware controller. The project successfully yielded a real-time simulation, demonstrating efficient generation and rendering of diverse celestial bodies and stable orbital physics, prioritizing consistent frame times. This work contributes a practical implementation and analysis of techniques for large-scale procedural galaxy simulation in Godot.


#text(lang: "SE")[
  #heading(numbering: none, outlined: false, bookmarked: true)[
    Sammandrag #text(red)[JONATAN KLAR]
  ]
  Processuell innehållsgenerering erbjuder kraftfulla metoder för att skapa vidsträckta virtuella universum, vilket är särskilt relevant för rymdutforskningssimulationer. Metoden medför dock betydande utmaningar gällande beräkningsmässig skala, prestanda och trovärdighet. Detta projekt adresserar dessa utmaningar genom att utveckla och simulera en fysikbaserad, processuellt genererad och utforskningsbar galax i spelmotorn Godot. Huvudmålet var att skapa en deterministisk och beräkningseffektiv modell. Planeter genererades med hjälp av brusfunktioner (Perlin och fBm) i kombination med Marching Cubes-algoritmen för att skapa komplex terräng. Denna terräng förbättrades sedan med processuellt placerade hav, vegetation och fysikbaserade atmosfärer som simulerar Rayleigh-spridning. Eftersom optimering var avgörande användes tekniker som flertrådning för planetgenerering, "chunking" och detaljnivåer (LOD). En specialbyggd N-kropps-fysikmotor simulerar himlakroppars mekanik. Motorn använder en parallelliserad Barnes-Hut algoritm ($O(N "log"N)$), byggd på ett linjärt octree baserat på Morton-kod (implementerat i Rust via GDExtension). Solsystem och galaxens struktur baseras på "seedad" slumpmässighet för att säkerställa determinism och reproducerbarhet. Utforskning är möjlig på flera skalor, från ett gränssnitt med en galaxkarta ner till navigering på planetens yta med fysikbaserad styrning. Projektet resulterade i en framgångsrik realtidssimulering som demonstrerar effektiv generering och rendering av varierande himlakroppar samt stabil omloppsfysik. Fokus låg på att uppnå konsekventa "frame times". Detta arbete utgör ett praktiskt bidrag i form av en implementation och analys av tekniker för storskalig processuell galaxsimulering i Godot.
]


#pagebreak()
#heading(numbering: none, outlined: false, bookmarked: true)[
  Acknowledgements
]

We would like to thank our supervisor Staffan Björk for his invaluable support throughout the project. From telling funny anecdotes to being a mentor, he has always been there to provide many interesting ideas and answers to even our most difficult questions. 


#pagebreak()
#[
  #show outline.entry.where(level: 1): set text(
    weight: "bold",
    size: 12pt,
  )
  #outline(
    indent: 2em,
  )
]

#pagebreak()
#heading(numbering: none)[
  Figures
]
#outline(title: none, target: figure.where(kind: image))

#pagebreak()
#heading(numbering: none)[
  Tables
]
#outline(title: none, target: figure.where(kind: table))

#pagebreak()
#heading(numbering: none)[
  Glossary
]

- *Mesh:* A collection of vertices, edges, and faces that define the shape of a 3D object.
  - *Vertex:* A point in 3D space, representing a corner of a triangle.  Stores attributes like position (x, y, z), normal, UVs, and optionally color.
    - *Normal:* A vector that is perpendicular to a surface. Used in lighting calculations.
    - *UV coordinates:* a 2D coordinate system used to map a texture image to a 3d object surface.
  - *Edge:* A line segment connecting two vertices.
  - *Face (Triangle):* A flat surface bounded by edges (typically a triangle in real-time graphics). Triangles are used because they are always planar.
  
- *Rendering Pipeline:* The sequence of stages a GPU uses to render a 3D scene.

- *Shader:* A small program that runs on the GPU. Primarily used for controlling how objects are rendered.
    - *Vertex Shader:* Processes each vertex; transforms positions from model space to clip space and manipulates vertex attributes.
      - *Model Space:* The object's local coordinate system.
      - *Clip Space:* A standardized coordinate system used by the GPU after vertex transformation.
    - *Fragment Shader (Pixel Shader):* Determines the final color of each fragment, often using lighting and textures.
      - *Fragment:* A "potential pixel" generated during rasterization.
      - *Rasterization:* The process of converting vector graphics (triangles) into raster graphics (pixels).
    - *Compute Shader:* Used for general-purpose computation on the GPU (not directly part of the rendering pipeline).

- *Procedural Generation:* The creation of data (models, textures, etc.) algorithmically, rather than manually. This often uses noise functions and other mathematical techniques to create varied and complex results.
  - *Noise Function (Perlin, Simplex):* Algorithms for generating pseudo-random values with a smooth, continuous appearance, used for procedural generation.
  - *Marching Cubes:* An algorithm to create a triangle mesh from a 3D scalar field (e.g., noise data).

- *LOD (Level of Detail):* Rendering objects with varying complexity based on distance.
- *PCG*: Procedural content generation
- *Axis Aligned Bounding Box (AABB):*  A box where each face is aligned to a basis vector.
#pagebreak()

#set page(numbering: "1")
#counter(page).update(1)

= Introduction #text(red)[ERIK klar]
Procedural generation offers a way to algorithmically create vast and diverse game worlds without the immense manual effort required to design every detail. Applications of procedural generation range from the random placement of enemies in confined dungeon spaces to the generation of entire universes comprising millions of celestial bodies. Using procedural content generation (PCG) also has the potential to increase re-playability #cite(<PCGNeuroevolution>).

Using PCG algorithms is particularly relevant in the context of space exploration games, where the scale of the universe is inherently beyond manual creation. Creating compelling, varied, and believable planetary systems and galaxies is a challenging problem within this domain. Key issues include the computational efficiency required to generate hundreds to millions of celestial objects, as well as the need to balance performance constraints with the goal of providing a plausible and playable experience. 

This project aims to address the technical and creative challenges of building such systems in a computationally efficient manner, while also simulating them within a game engine, such as Godot. Implementation requires utilization of techniques within the area, such as utilizing the power of GPU computations and different terrain generation algorithms. In addition, the project hopes to provide insight into how such challenges can be approached by documenting the thought process behind decisive development decisions.

== Purpose #text(red)[Jacob klar] <purpose-ref>
The aim of this project is to create a physics-based simulation of a procedurally generated, explorable galaxy. 

Each solar system that make up the galaxy will consist of various procedurally generated planets, orbiting a central star. These orbits are governed by a simplified physics simulation based on Newtonian physics. System complexity can vary, ranging from a sun with a single planet, to arrangements with multiple planets and moons.

While procedurally generated, the galaxy will remain consistent and revisitable by ensuring deterministic generation. Different seeds will allow for unique galaxies to be created, while also enabling parts to be generated identically, upon revisit.

== Limitations
Some limitations for the projects have been set. The physics simulation will be simplified and not necessarily accurate according to real laws of gravity. This does not mean that the program will completely disregard the accuracy of the physics model, but will instead focus on specific aspects such as the orbit of celestial bodies and how they affect each others orbits. The system needs to be realistic enough to simulate solar systems, but not realistic to a point where all intricacies of physics will be considered.

Furthermore, the project will not be developed into a full scale video game. Rather, the focus will lie on the more technical aspects: terrain generation, physics, and performance optimization. Other features such as a UI for quick travel or detailed planet properties are not prioritized. Essentially, the goal of the project is to create a model of a procedurally generated galaxy, not an engaging game play experience.

== Contribution #text(red)[Erik Klar]
This report hopes to contribute with a computationally efficient, conceptually interesting and accurate physics model for simulating celestial movements in the game engine Godot.

= Background
This section presents the foundational theoretical concepts that were needed before beginning the project, followed by a few select previous works that utilize some of these concepts.

== Procedural Content Generation #text(red)[William klar]
Procedural Content Generation (PCG) is defined as “the algorithmic creation of game content with limited or indirect user input”@shaker2016procedural. In video games, this typically involves the automatic generation of content such as unique levels for each gameplay session or the stochastic placement of environmental elements like vegetation. Throughout the project procedural generation will be used in variation. 

== Noise #text(red)[ANTON typ KLAR]
Noise is very commonly used in a plethora of computer generated content, such as for generating mountains, textures and vegetation @proceduralgame:2016. Noise is generated through the use of a pseudo random function and is often stored and visualized as a texture. Perlin noise @perlinnoise:1985 is a famous gradient noise function founded by Ken Perlin in 1985 that has been used extensively to produce procedural content in games and films. Noise will be at the core of many of the things developed in this project and will be used frequently.

== Terrain Generation #text(red)[William klar]
This subsection provides an overview of height maps and the marching cubes algorithm, the two techniques used to procedurally generate terrain in the project.

=== Height maps #text(red)[William klar]<height-maps>
Height maps serve as data structures that encode elevation information for each vertex within a mesh's geometry @heightmaps:2019. Typically, height maps are implemented as grayscale image files, where the intensity of each pixel corresponds to the height at a specific location on the mesh. This representation is computationally efficient and widely used in terrain generation and visualization. However, height maps have inherent limitations. Notably, they are capable of representing only a single elevation value per (x, y) coordinate. As a result, they are unsuitable for modeling complex terrain features that involve vertical structures or overlapping geometry, such as caves, arches, or overhangs.

#align(center,
  grid(
    columns: 2,
    gutter: 30pt,
    [
      #figure(
        image("images/Heightmaps/Heightmaptexture.png", width: 160pt, height: 160pt),
        caption: [Heightmap texture]
      )
    ],
    [
      #figure(
        image("images/Heightmaps/Heightmap_rendered.png", width: 250pt, height: 160pt),
        caption: [3d render using the heightmap texture]
      )
    ]
  )
)

=== Marching Cubes #text(red)[William klar]<marching-cubes>
The Marching Cubes algorithm @marchingcubes:1998 is a method for extracting a polygonal mesh representation of an isosurface from a three-dimensional scalar field. In essence, it receives a grid of scalar values as input and produces a mesh that approximates the isosurface defined by a specified threshold value. Initially developed for the visualization of medical imaging data such as CT and MRI scans, the algorithm has also found recent applications in procedural mesh generation within computer graphics.

The procedure involves traversing the scalar field and evaluating groups of eight adjacent grid points, which collectively form a logical cube. For each cube, the algorithm determines the polygon(s) that approximate the isosurface intersecting that region. This is achieved by classifying each vertex of the cube as either inside or outside the isosurface, based on whether its scalar value is above or below a predefined iso-level. The algorithm then references a precomputed lookup table to identify the appropriate triangulation for the cube's configuration.

Given that each of the eight cube vertices can exist in one of two states (inside or outside), there are $2^8 = 256$ possible configurations. However, due to symmetry and rotational equivalence, these reduce to 15 unique cases (see #ref(<mc15>)), with all others derivable through reflection or rotation. The process is repeated throughout the entire scalar field, and the resulting polygons are aggregated to form the final mesh.
#figure(
  image("images/MarchingCube/MarchingCubesEdit.svg"),
  caption: [Marching Cubes 15 unique polygon combination. Ryoshoru, #link("https://creativecommons.org/licenses/by-sa/4.0")[CC BY-SA 4.0], via Wikimedia Commons]
) <mc15>

== Chunks #text(red)[William klar]
Chunks refers to (in this context) a fixed-size segments of data that is loaded, processed, and rendered independently. This approach is particularly beneficial in large or procedurally generated environments, as it allows the game engine to manage memory and computing resources efficiently by loading only the chunks near the player.

Chunking can be used for additional optimization such as:
- Frustum Culling: Rendering only the chunks within the player's field of view.​
- Occlusion Culling: Skipping the rendering of chunks or objects that are blocked from view by other objects.​
- Level of Detail (LOD): Reducing the complexity of distant chunks to save on processing power.

=== Octrees #text(red)[William klar]<B-octree>
An octree is a hierarchical data structure that recursively subdivides three-dimensional space into eight octants using a tree structure (see #ref(<octreeimg>)). This structure is particularly effective for managing sparse or large-scale environments, as it allows for efficient spatial queries, collision detection, and level-of-detail (LOD) rendering.​ @octree1

#figure(
  image("images/Octree/Octree2.svg.png", width: 65%),
  caption: [Visulaizaiton of a Octree in both a square- and tree format. WhiteTimberwolf, #link("https://creativecommons.org/licenses/by-sa/3.0/deed.en")[CC BY-SA 3.0], via Wikimedia Commons]
)<octreeimg>
While chunks are typically uniform, fixed-size sections of the game world loaded and unloaded as needed, octrees offer a more dynamic approach. In some implementations, each leaf node of an octree represents a chunk, allowing for variable levels of detail within different regions of the game world. This integration enables efficient memory usage and rendering performance, especially in procedurally generated or expansive environments.
== GPU Computation #text(red)[Erik]
GPU computing is the process of utilizing the highly parallel nature of the GPU for running code. Since the GPU has significantly more processing units than the CPU @princeTonGPU, it can be utilized to write highly parallelized pieces of code to solve certain programming problems. These programs that run on the GPU are often referred to as compute shaders @UnityComputeShaders.

== Previous works #text(red)[William klar]
Several existing games and research projects provide a foundation for this work, demonstrating both the potential and the challenges of procedural planet and solar system generation and simulation:

=== Minecraft
While not focused on planetary systems, Minecraft @minecraft:2009 demonstrates the power of procedural generation for creating vast and varied landscapes using noise. This random generation results in virtually endless environments, drawing players in with the offer of exploring new, never seen before areas. Minecraft's success highlights the appeal of procedurally generated worlds, and the endless possibilities they offer for creativity and exploration.

#figure(
  image("images/PreviousWorks/minecraft_landscape.png", width: 65%),
  caption: [Minecraft Windswept Hills biome]//@minecraft_landscape]
)


=== No Man's Sky
No Man's Sky @hello_games:2016 famously utilized procedural generation to create a universe of planets, each with unique flora, fauna, and landscapes. It demonstrated the potential of large-scale procedural planet generation in a commercial game.

#figure(
  image("images/PreviousWorks/nomanssky.png", width: 65%),
  caption: [No Man's Sky planet surface]//@nomanssky_planet]
)


=== Outer wilds
Outer Wilds is a space exploration and adventure game that bears a small resemblance to this project. Unlike the other works mentioned, which are related through their use of procedural generation, Outer Wilds features entirely hand-crafted content. Its relevance to this project instead lies in its approach to physics. In Outer Wilds, all physics interactions are computed in real time, with no pre-defined behaviors. For instance, planetary motion is governed by a modified version of Newton's law of gravitation, and all velocities are dynamically calculated during gameplay.
@outerwilds1
#figure(
  image("images/PreviousWorks/outerwilds_mech_3.jpg", width: 65%),
  caption: [Map of the Outer Wilds solar system]
)

=== Exo Explorer
Exo Explorer @exo_exporer:2023 is an earlier bachelor thesis project, also from Chalmers, directly addressed the challenge of procedurally generating solar systems using the Unity engine. The project utilized Perlin noise @perlinnoise:1985 and the marching cubes algorithm @marchingcubes:1998 to create planet terrain featuring forests, lakes, and creatures with basic simulated needs (hunger, thirst, reproduction). 

Exo Explorer will serve as a valuable source of inspiration for this project, demonstrating techniques of procedural generation, optimization, etc, that this project aims to explore as well. Whilst aiming to delve deeper into other aspects such as the complexity of the simulated physics, performance, and exploration; especially with a greater focus on simulating several solar systems at the same time for the user to explore.

#figure(
  image("images/PreviousWorks/exoexplorer.png", width: 65%),
  caption: [Cover photo from the #link("https://github.com/Danilll01/Kandidatarbete2023?tab=readme-ov-file")[#underline[project's repository]]]
)

= Method and Planning #text(red)[Fixa tempus till typ was planned to]<Method>
This section describes the methodology and planning behind the project. It presents the chosen workflow, the intended features, selected tools and technologies, as well as considerations related to societal and ethical implications.

== Workflow
Development followed an Agile adjacent workflow @Agile101, meaning that the work was divided into "sprints" with iterative task refinement. Task prioritization and addition of tasks to the backlog was be done during the end of the week before the weekly supervisor meeting. Task management involved tracking various states for each task (on the Kanban board), including "blocked", "todo", "in progress", "in review", and "done". All labels are self-explanatory except for "blocked"; tasks categorized under this label cannot be worked on before prerequisite task(s) are finished. 

== Git
During the development process, the version control system Git was utilized in conjunction with Github. Additionally, the Github repository served as a platform for task management by employing a Kanban board to facilitate tracking of task assignments and progress.

The projects standard workflow for Git and GitHub involves maintaining each feature within a dedicated branch. GitHub’s Kanban board allowed us to associate branches with specific tasks, ensuring a clear and structured development process. Moreover, acceptance criteria were established for each task on the Kanban board. Once all criteria were met, a pull request was created to merge the changes into the main branch. Before finalizing the merge, at least one other team member was required to review the code and the feature. This review process served both as a quality assurance measure and as an opportunity to provide constructive feedback.

== Godot
The Godot game engine is the engine that was chosen for this project. Godot is a free and open-source game engine recognized for its lightweight architecture, efficient scene system, and user-friendly interface. It employs a node-based system that enables modular and reusable component design. The engine officially supports multiple programming languages, including GDScript, C\#, and C++. @GodotFeatures Furthermore, community-driven extensions, detailed in @GDExtension, expand language compatibility beyond these officially supported options.

As mentioned, everything is built using what are called Nodes. A *Node* is a fundamental building block for creating game elements, and it can represent various components such as an image, a 3D model, a camera, a collider, a sound, and more. Together, nodes form a *Tree*, and when you organize nodes in a tree, the resulting assembly is called a *Scene*. Scenes can be saved, and reused as self-contained nodes, allowing them to be instantiated in different parts of the application @godot-nodes-and-scenes. For example, a Player character might consist of multiple nodes, such as an image, collider and camera. All grouped together, they form a Player Character Scene, which can then be reused wherever needed.

To minimize conflicts during the merging process in Git, concurrent modifications to the same scene will be limited. This practice helps prevent merge conflicts, which can otherwise introduce inefficiencies and complications in development.

Godot was chosen over other engines because it is a light weight engine compared to others (such as Unity and Unreal Engine); it does not have heavy pc requirements which means that it can easily work on lower end machines. 
Additionally, it's support for multiple languages gives support to more performance efficient languages such as C++ or Rust, which can help with optimization. 
Lastly, as a relatively new engine, it was deemed to be worth learning from an academic perspective. 

== Benchmarking and Performance #text(red)[Jacob klar] <benchmarking-and-performance-ref>
When developing real-time applications such as simulations, video games, or other computer applications, maintaining responsiveness and stability during runtime is essential for the user experience. A common metric to measure the performance of any such application is Frames Per Second (FPS), which is the amount of rendered images (frames) that are displayed each second. Higher and consistent FPS is desirable for a stable experience, as well as reduced visual artifacts, and improved system latency from when a user inputs, to it being represented on the display. @nvidia_fps

However, FPS alone does not always provide a complete picture of performance. Instead, the time it takes to render each frame (frame times) is considered instead. Frame times reveal inconsistencies during runtime, such as brief momentary lag at computation heavy moments. These moments may be overlooked in average FPS values, while being detrimental to the user experience. Metrics such as 1% lows and 0.1% lows of FPS (highs of frame times) have become common to expose these worst-case scenarios, capturing the average of the slowest (highest value) 1% and 0.1% of all frame times respectively.

The disparities between the 3 values of: the total frame time average, the 1% lows, and the 0.1% lows, are what is important. Reducing the disparities between each other is what is crucial for an overall stable user experience. Gamers Nexus@gamers_nexus_youtube_fps_lows mentions that disparities between frames of 8ms or more, are what is starting to become perceptible to the user.

This approach is detailed in NVIDIA's developer guides@nvidia_frametimes, explained by Gamers Nexus@gamers_nexus_youtube_fps_lows, employed in benchmarks by Gamers Nexus@gamers_nexus_dragons_dogma_benchmark, with underlying work in "Inside the Second" by Scott Wasson@techreport_inside_the_second.

All in all, this project will put emphasis on maintaining consistent frame times, rather than high FPS, more precisely:
- Keep the disparities between the frame time average, the 1% lows, and the 0.1% lows, to a maximum of 8ms.
- Maintain an average 30 FPS (frame times of 33.33ms), when the program is not experiencing its 1%, or 0.1% lows.
This, on a dedicated benchmarking computer of the following specifications:

SPECS

== Planning
This sub section provides an overview of the identified task and sub tasks to complete the project, the planned features to be implemented as well as a rough timeline for the project.
// gantt schema
#let gantt = timeliney.timeline(
  show-grid: true,
  {
    import timeliney: *
    headerline(
      group(([*Study Period 3*], 9)), 
      group(([*Study Period 4*], 9))
    )
    
    headerline(
      group(([Jan], 1)),
      group(([Feb], 4)),
      group(([Mars], 4)),
      group(([April], 4)),
      group(([May], 4)),
      group(([Jun], 1)),
    )
    
    headerline(
      group(..range(8).map(n => text(size: 8pt)[*SW#str(n + 1)*])),
      group((text(red, size: 7pt)[*Exam*], 1)),
      group(..range(8).map(n => text(size: 8pt)[*SW#str(n + 1)*])),
      group((text(red, size: 7pt)[*Exam*], 1)),
    )
  
    task(
      "Project Plan",
      (0, 4),
      style: (stroke: 10pt + olive)
    )
    task(
      "Development",
      (4, 15),
      style: (stroke: 10pt + eastern)
    )
    task(
      "Presentation",
      (7, 8), (16, 17),
      style: (stroke: 10pt + yellow)
    )
    task(
      "Write Report",
      (4, 17),
      style: (stroke: 10pt + maroon)
    )

    milestone(
      at: 17.3,
      style: (stroke: (dash: "dashed")),
      align(center, [
        *Final Deadline*\
        Mon, Jun 2
      ])
    )
  }
)

=== Task #text(green)[ERIK KLAR]
This chapter outlines the key tasks identified during the planning phase of the project. These have been categorized into four main areas: _procedural generation_, _physics simulation_, _exploration_, and _optimization_. Each category encompasses specific objectives and implementation considerations necessary to achieve the project’s goals.

_*Procedural Generation*_

As previously discussed, procedural generation refers to the algorithmic creation of content such as terrain and planetary systems. This represents a core component of the project, allowing for the generation of planets, solar systems, and entire galaxies. To ensure determinism and reproducibility, all procedural content was designed to be generated using a fixed seed.

_*Planets*_

It was decided that the planets should primarily be generated using different types of noise. The planets should also be given shape as explained earlier using the marching cube algorithm. Furthermore, the planets should be generated to be aesthetically distinct from on another.

_*Solar System*_

The solar systems were planned to be generated using different randomized parameters such as:
- Number of planets in the system
- Orbit of celestial bodies.
- Attributes of celestial bodies 
  - Mass
  - Rotation
  - Size
Certain parameters, such as orbital radius, mass, and velocity, were fine-tuned manually to ensure system stability. This approach would allow for the possibility of generating coherent systems in real time.

_*Physics Simulation*_

Accurate gravity simulation was a central requirement for achieving realistic physical behaviors. Different gravity systems were planned to be utilized for different purposes. For example, the player system could utilize the built in physics engine to simulate gravity on planets, while the celestial bodies and solar systems could utilize a manual implementation that is better suited for them.

_*Exploration*_

An ability to explore the generated content needed to be implemented. Multiple solutions were discussed, but since the main purpose wasn't to provide an engaging game play experience, it was decided that a simple camera controller would be prioritized, with the option to add more complex features if time allowed for it. Additionally, functionality for inter-solar system traversal needed to be implemented to allow for full exploration possibilities of multiple solar systems.

_*Optimization*_

The aim is to construct a real-time application, and thus, optimization techniques are increasingly important. Inefficient algorithms and resource management will eventually lead to performance bottlenecks. To address this, proven techniques and smart solutions were explored throughout all parts of the project to improve overall efficiency.

=== Features <features>
Features that were planned to be implemented were prioritized using the MoSCow analysis method @moscowprio:2018 (see #ref(<MosCow>)). This was done in order to better understand which features to prioritize and which to save for later.

#figure(
  table(
  columns: (auto, auto, auto),
  gutter: 5pt,
  inset: 7pt,
  align: left,
  table.header(
    [*Must Have*], [*Should Have*], [*Could Have*]
  ),
  [
    - Working terrain-generation using computer generated noise.
    - There should be at least one procedurally generated planet and sun.
    - The planet should orbit the stars with as accurately as possible to the laws of physics.
    - A way to explore the generated terrain using a camera controller.
  ],
  [
    - To explore different performance techniques to achieve effective procedural generation of planets
      - Occlusion, backface, etc, -culling
      - Levels of detail (LOD)
      - Applied spatial data structures (E.g octrees and grids).
      - Chunking
    - A technique to draw complex terrain with overhangs and the like, e.g Marching cubes.
    - Solar systems can have multiple planets.
    - Some planets should have moon(s).
    - Galaxy. A procedurally generated galaxy constructed out of solar systems
    - Galaxy traversal. Ways of navigating between solar systems, using a UI galaxy map or traveling from solar systems in real time. Zooming out: Solar system level, solar systems in proximity, galaxy level. (examples)
    - There should be shaders to improve the look of things in the solar system, such:
      - Planet atmospheres
      - Water on planets
    - Player abides by the laws of physics, interacting with planets by walking for example.
  ],
  [
    - Get interesting info about planets and solar systems, such as information about a planet's mass, amount of moons
    - Planet properties like temperature that change according to, for example, distance from the sun. Hot at equator, cold at poles, etc.
    - UI for navigating between planets/solar systems/galaxies (fast travel)
    - Other celestial bodies. E.g. asteroids, black holes, asteroid belts, gas planets, nebulae, etc.
    - Vegetation
  ],
  ),
  caption: [The projects MoSCow Table],
)<MosCow>
#pagebreak()
=== Timeline
This subsection provides a rough timeline for the project (see #ref(<Gantt>)).
#figure(
  // image("gantt_rev2.png", width: 90%),
  gantt,
  caption: [Gantt Chart]
) <Gantt>

== Societal and Ethical aspects #text(red)[ERIK KLAR]
The two main points of discussion regarding ethical and societal aspects that are deemed to be relevant are how procedural content generation in game development affects game designers, mainly focusing on level designers, and how players might be affected by procedural content generation in games.

Game designers within game development might lose their relevance if the procedural generation and the use of AI gets precise enough, meaning that the algorithms can perfectly replace human developers. Even though procedural content generation can help game companies reduce development cost and time#cite(<computers13110304>), the concerns that the algorithms proficient enough to replace human creativity are still present. An example for this is when the Swedish game company Mindark announced plans to fire half of their employees, primarily world builders, in favor of AI-driven content generation#cite(<MindarkAftonbladet>).

The procedural content generation must be interesting enough and playable to not negatively affect players. Games containing procedural content generation are at the risk of containing repetitive content, which may influence a player's sense of immersion or reduce re-playability. An example where the content generation affected the game play negatively is when the game "No Man's Sky" was released . The planets generated by the game ended up being too repetitive and basic @pcgchallanges:2017. Additionally, PCG systems may inadvertently create environments that hinder gameplay, such as untraversable terrain, thereby negatively affecting the overall playability of the game.

= Process #text(red)[SKRIVEN AV NÅGON, ERIK? KLAR]
This section outlines the process for creating the various components that comprise the project. Each subsection represents a step in increasing scale - starting from the planet-scale, focusing on unique terrain generation and other planetary features, expanding to the system-scale organization of celestial bodies and their orbital physics, and finally reaching the galaxy-scale distribution of stars.

Unless stated otherwise, all figures shown in this section were produced by ourselves.

== Planet Generation #text(red)[ANTON] <planet-gen-ref>
This section will describe the process how the planet generation was implemented.

=== Height-map planets #text(red)[ANTON KLAR]<heightmap-planets>
The first planets to be constructed was the height-map planets. They where very simple and did not offer any complex terrain. These planets were made  mapping a cube onto a sphere and utilizing height maps (@height-maps) to create variation in the terrain elevation; using the values stored in the height-map to displace the vertices of the sphere. This worked well to start with, but the goal was to create more advanced planets that utilized the marching cubes algorithm and 3D noise in order to get more "interesting looking" terrain, including for instance overhangs and caves. Additionally, a simple planet shader was implemented to add visual interest by coloring the planets based on their height relative to the lowest point.

#align(center,
  grid(
    columns: 2,
    gutter: 30pt,
    [
      #figure(
        image("heightmap-planet.png", width: 160pt, height: 160pt),
        caption: [Height-map planet with shader]
      )<fig:heightmap-planet>
    ],
    [
      #figure(
        image("heightmap-planet-wireframe.png", width: 160pt, height: 160pt),
        caption: [Wireframe of height-map planet]
      )<fig:heightmap-planet-wireframe>
    ]
  )
)

=== Transitioning from height-maps to marching cubes #text(red)[ANTON KLAR, WILLIAM SKRIVER]<transition>
When transitioning from height-maps to marching cubes, the method of generating the planets needed to change from a cube mapped onto a sphere to a collection of equidistant points in 3D space, with each point containing a noise-value. These points was then to be used as input to the marching cubes algorithm (@marching-cubes). The points were constructed as a simple 3D-matrix and @fig:noise-cube shows what these points could look like when visualized using small spheres.

#figure(
  image("images/PlanetNoise/noise.png", width: 160pt, height: 160pt),
  caption: [Cube of points]
)<fig:noise-cube>

Each point in the noise field gets assigned a noise-value which represents whether or not it's "inside" or "outside" the object. These points and corresponding value are then passed to the marching cubes algorithm to construct the mesh.

- *Skriv om hur vi implementera Marhing Cubes*
- *Lägg till pseudo-kod i appendix*

=== Noise planets #text(red)[ANTON KLAR]<noise-planets>
The noise planets were the first version of planets utilizing marching cubes. These planets were formed by generating a 3D matrix of points as shown in @transition and assigning a value to all points. Initially, the values given were binary in nature, with ```typst -1``` representing that point as being "outside" the surface (or considered as "empty space"/"above the ground") and ```typst 1``` representing the point being "inside" the surface (or as "included in the surface geometry"/"under the ground"). In order to make the planets spherical, the value of ```typst 1``` was given to all points that were inside some defined radius from a the planet's center point and all other points was given the value of ```typst -1```.
    ```cs
                float distanceToCenter = (centerPoint - currentPoint).Length();
                if (distanceToCenter < radius) {
                    points[x, y, z] = 1.0f;
                } 
                else {
                    points[x, y, z] = -1.0f;
                }
    ```

The method just described produced a smooth spherical planet as can be seen in @fig:noise-planet-1. There were no interesting terrain being generated, so to fix this issue, noise was used. Instead of assigning constant values for the points being inside the sphere, a 3D noise function was used instead. In order to give variation to each generated planet, a random offset was also introduced when sampling the noise to avoid generating two identical planets. The below code shows how this was done and the result can be seen in @fig:noise-planet-2.

    ```cs
                if (distanceToCenter < radius) {
                    points[x, y, z] = GetNoise3Dv(currentPosition + offset);
                } 
    ```

Although the terrain did become more visually interesting when using noise, the planet depicted in @fig:noise-planet-2 does not necessarily appear as visually appealing as the hight-map planets from earlier. The reason for this is partly due to the fact that the planets were forced to be completely spherical and all detail was simply "carved out" from the inside of the planet.

#align(center,
  grid(
    columns: 2,
    gutter: 20pt,
    [
      #figure(
        image("images/PlanetNoise/noise_planet_sphere.png", width: 160pt, height: 160pt),
        caption: [Smooth planet, generated using constant values]
      )<fig:noise-planet-1>
    ],
    [
      #figure(
        image("images/PlanetNoise/image.png", width: 160pt, height: 160pt),
        caption: [Noise planet using 3D Perlin noise],
      )<fig:noise-planet-2>,
    ]
  )
)

=== fBm planets #text(red)[ANTON typ KLAR]<fbm-planets>
The noise planets (@noise-planets) were a step in the right direction, but they lacked surface variation and appeared more like carved-out spheres. The initial idea to address this was to simply apply a height map to all vertices in the same way as in @heightmap-planets. However, it was quickly realized that this would potentially require three separate passes of the planet data; once to generate the data points, once during the marching cubes algorithm and finally, one more time to loop over all vertices of the newly created mesh. Furthermore, this approach would also eliminate the possibility of generating overhangs and more complex terrain features above ground level.

Thus, research was conducted to find a better approach and eventually a method called fractional Brownian motion was identified and later utilized to add more complexity and detail to the terrain, making the planets more aesthetically appealing.





Fractional Brownian motion (fBm) is a technique that is often used in computer graphics and the video games industry for generating realistic-looking terrain @fbm-chow. 

The idea is to layer several "octaves" of noise, each with increasing frequency and decreasing amplitude to produce smaller and smaller details. Consider a sine-wave, if one where to add another sine-wave to the first one, the amplitude of both would be added together to form a sine-wave oscillating from negative two to two. If the second sine-wave had a larger frequency than the first but a lower amplitude, then its "height contribution" would become smaller but it would instead add smaller detail to the end result:

```cs
              float firstOctave = sin(x);
              float secondOctave = sin(x) + sin(2*x)*0.5;
              float third octave = sin(x) + sin(2*x)*0.5 + sin(4*x)*0.25;
```

The lower octaves have the same appearance as the higher octaves but on a smaller scale, due to fBm being self similar. This property is useful when generating natural-looking terrain because it ensures that the detail will scale consistently and that the result will blend well together.

The implementation of fBm in this project was straight forward, and instead of using sine-waves, the 3D noise from before was used:

```cs
Vector3 currentPosition = new Vector3I(x, y, z);
float distanceToCenter = (centerPoint - currentPosition).Length();
float distanceToSurface = (float)radius - distanceToCenter;
points[x, y, z] = Fbm(distanceToSurface, currentPosition, param, fastNoise);;
```

In the fBm function, there is a single for-loop which, for each octave, transforms the previously used point-value by adding noise with increased frequency and decreased amplitude. Lacunarity is the amount that the frequency should be changed by each octave, and persistence is the amount the amplitude should be multiplied by each octave. Typical values for these are ```cs lacunarity = 2``` and ```cs persistence = 0.5``` due to generating natural-looking terrain @quilez2019fbm. However, both of these parameters are randomly chosen based on logic later described in @proc-gen in order to add more variation to the planets; to broaden the types of planets that can be generated.

#text(red)[KANSKE TA UPP SEN - in the papers, a hurst exponent is talked about... (main part)... this is related to the persistence which is used here. Commonly 2^(-h) (h is [0,1]) is used and h=1/2 => regular brownian motion.]

#figure(
  block[
    ```cs  
      float Fbm(float fbmValue, Vector3 pos, FastNoiseLite noise) {
          for (int i = 0; i < octaves; i++) {
              fbmValue += noise.GetNoise3Dv(frequency * pos + offset) * amplitude;
              amplitude *= persistence;
              frequency *= lacunarity;
              offset += new Vector3(random.NextInt(octaves));
          } return fbmValue; 
      }```
  ],
  caption: [Fractional Brownian Motion implementation (simplified for readability) used in planet generation]
)<fbm-code>

Using fBm, the planets were further improved, with more varied terrain and, most importantly, variation in the elevation. The result of these changes can be seen in @fig:fbm-1 and @fig:fbm-2 below.

#align(center,
  grid(
    columns: 2,
    gutter: 55pt,
    grid.cell([
      #figure(
        image("images/PlanetNoise/fbm_planet_1.png", width: 160pt, height: 160pt),
        caption: [fBM planet],
      )<fig:fbm-1>
    ]),
    grid.cell([
      #figure(
        image("images/PlanetNoise/fbm_planet_2.png", width: 160pt, height: 160pt),
        caption: [fBM planet with flat area],
      )<fig:fbm-2>
    ]),
  )
)

#block[
  _Note_: Between @noise-planets and @fbm-planets, interpolation was introduced to the marching cubes algorithm, which causes the planet terrain in @fig:fbm-1 and @fig:fbm-2 as well as _all subsequent figures_ to appear smoother compared to the terrain depicted in @noise-planets. #text(purple)[Är detta bra?]
]



There where some issues with these planets however, as can be seen in the center of @fig:fbm-2, where sometimes the noise function together with the fBM algorithm can cause the boarders of the planet to become part of the planet geometry in the marching cubes algorithm, which can make flat areas on the surface of the planet, or just cut of entire areas.

TODO:
This was fixed by tex... 
- dynamiskt förstora, arrayen
- dynamiskt skala om radien


=== Procedural planet generation #text(red)[ANTON KLAR] <proc-gen>
The next step after creating the planet generation, were to procedurally generate the planets at run-time. This was done by manually experimenting with the fBm parameter values until ranges that produced visually satisfactory results (as judged by the developers) was identified. Then, the parameters was randomized, according to the logic presented in the following pseudo-code:

```cs
void RandomizeParameters() {
    Random random = new Random();

    // NextInt(a,b) returns a random int between a and b
    octaves = random.NextInt(4, 8);

    // NextFloat(a,b) returns a random float-value between a and b
    amplitude = random.NextFloat(0.0f, 20.0f);

    if(amplitude < 4)   frequency = random.NextFloat(0.0f, 12.0f);
    else                frequency = random.NextFloat(0.0f, 2.0f);

    if (frequency < 4)  lacunarity = random.NextFloat(0.0f, 4.0f);
    else                lacunarity = random.NextFloat(0.0f, 8.0f);

    persistence = random.NextFloat(0.1, 0.25);
}
```
However, it was found that this method was non-deterministic, meaning that on subsequent visits to the same solar system, the planets would not look identical to those seen during the first visit. To address this issue, individual planet seeds derived from the planet's solar system seed were introduced. The following pseudo-code shows how the system seed and the planet's starting position is used to calculate a new planet seed (using the method described later in @seed-ref) which is then used to update the seed of the ```typst Random``` variable from earlier:
```cs
    Random random = new Random();
    int planetSeed = GenerateSeed(systemSeed, planetPosition)
    random.seed = planetSeed;
```

=== Coloring the marching cubes planets #text(red)[ERIK KLAR]
The code for coloring the planets was reused from the first planet implementation, with one extension. Cliff edges could be simulated by calculating the dot product between the  direction of a specific vertex normal and the direction to the planet center. If the resulting value was close to one, the corresponding fragments at that position were assigned the cliff color (see #ref(<fig:cliff-face>)). Some color themes were created in order to get aesthetically pleasing results (see examples in #ref(<PlanetColors>)).

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
      [#image("images/Planet/lavaWorld.png", width: 50%) c) Lava or Red Desert world]
    ),
    caption: [A few select color themes available when generating the planets.]
) <PlanetColors>

Each color theme was then given a specific value in the range [0,1] in order to simulate planet warmth, with 0 being the coldest and 1 being the warmest. These themes were grouped into sets based on their warmth values, which later enabled temperature-based coloring of planets according to their distance from the sun.

To simulate temperature falloff with increasing distance to the stars, the planet warmth was set using this formula:
$
"normalized_warmth"=("planet_warmth" - "min_warmth")/("max_warmth" - "min_warmth")
$
To further randomize appearance and to simulate the impact of atmosphere thickness on surface temperature, a chance that a neighboring theme set could be chosen was also added. For example, if a planet had a warmth of 0.5, there would be a chance that a theme set from a slightly higher or lower warmth could be chosen.



== Optimizing the Planet Generation #text(red)[William Klar] <planet-optimize-ref>
One of the changes introduced with the transition from height-maps to marching cubes is the increased computational power required to generate a mesh. This is due to the additional dimension involved in the mesh generation process. While height-maps only require a 2D texture to determine the height, as previously mentioned, marching cubes necessitate a 3D array, which significantly increases the computational scale.
 
=== Compute Shader #text(red)[William klar]
The initial optimization step involved transitioning the marching cubes generation process from the CPU to a compute shader.

Unlike the standard rendering pipeline, a compute shader operates independently and is invoked directly by the CPU. In contrast to other shader stages in the rendering pipeline that follow a clearly defined input-output structure, compute shaders utilize an abstract input model that is defined by the user. @openglcs

The primary motivation for employing a compute shader was the significant parallelization capabilities offered by the GPU. As previously noted, the marching cubes algorithm iterates through a grid of points, using the eight neighboring points at each step to construct a polygon. Since each iteration is largely independent of the others, the algorithm is inherently parallelizable— an area in which GPUs excel.

After transitioning the marching cubes algorithm to a compute shader-based approach, a compute shader was successfully implemented to generate a mesh using this algorithm. Following the implementation, performance testing was carried out to assess whether GPU-based mesh generation offered improved efficiency compared to the CPU-based approach.

This testing involved feeding identical grids of data points to both the CPU and GPU implementations of the algorithm and measuring the time required to generate the resulting mesh. The tests were performed across a range of different grid sizes to ensure broader applicability of the results.

Contrary to initial expectations, the GPU implementation demonstrated lower performance than its CPU counterpart. It is hypothesized that this outcome is primarily due to the overhead associated with buffer setup and retrieval, which is a known bottleneck in compute shader workflows. Additionally, the triangle buffer was configured to accommodate the worst-case scenario in mesh generation, assuming that each voxel could produce up to five polygons, thereby increasing retrieval time.

Furthermore, unlike Unity—which allows for indirect mesh creation directly within compute shaders to mitigate buffer-related overhead—Godot lacks such functionality. As a result, buffer retrieval must be performed manually, which introduces additional inefficiencies within the engine's pipeline. In the end, this approach did not achieve the intended reduction in planet generation time; however, as alternative methods remain available, the focus will now shift to exploring a different solution.

=== Worker Thread Pooling #text(red)[William klar]
An alternative approach involved partitioning the workload across multiple threads. Specifically, in addition to the main thread, a dedicated thread was introduced to handle planet generation requests. Previously, all operations were executed on the main thread, including a loop responsible for generating each planet during solar system creation. This process led to performance issues, as the computationally intensive planet generation caused noticeable stuttering; when the function to create a planet or solar system was invoked, the frame could not advance until the corresponding meshes had been fully generated. By offloading the generation tasks to a separate thread, the main thread could simply dispatch requests and proceed without delay. 

To further enhance this multi-threading strategy and reduce the overhead associated with repeatedly creating and destroying threads, the planet generation system was transitioned to use a thread pool. Since creating threads incurs considerable overhead and is relatively resource-intensive, it is desirable to minimize this cost—an objective that thread pools are designed to address. A thread pool functions by allocating a predefined number of threads at startup; in the context of Godot @threadpool2, this initialization occurs during project startup. When a task—such as planet generation—is submitted, it is added to a task queue. One of the pre-allocated threads (commonly referred to as workers) retrieves the task from the queue, executes it, and then proceeds to the next available task. This approach eliminates the need to create new threads for each operation, thereby improving efficiency and performance. @threadpool1

This adjustment significantly reduced stuttering between frames and allowed for a smoother experience.

=== Chunking & Level-of-detail #text(red)[William klar]
An additional optimization technique for planet generation involved implementing a chunking system. Chunking, as previously described, partitions data into equally sized segments. However, to further enhance performance, it was necessary to integrate a level-of-detail (LOD) mechanism. In this context, LOD entails reducing mesh complexity for distant planetary regions by using fewer data points during mesh generation—particularly relevant when employing the marching cubes algorithm. This reduction improves performance by enabling faster loading of less detailed planetary areas.

To support both chunking and LOD, an octree data structure was adopted. As referenced in #ref(<B-octree>), an octree recursively subdivides space into hierarchical nodes. Each leaf node in the octree represents a chunk of the planet, with the depth of the node determining the level of detail—shallower leaves correspond to lower resolution. Subdivision is driven by the player's proximity: as the player approaches a region, the corresponding node subdivides into eight higher-resolution child nodes, increasing local mesh detail dynamically. This approach ensures that only regions near the player are rendered in high detail, significantly improving efficiency. In summary, the octree effectively addresses both chunking and LOD requirements in a unified structure.

The implementation begins by initializing an instance of the _Octree_ class, using a size equal to the planet's diameter. A separate class, _OctreePlanetSpawner_, is responsible for handling mesh generation. Within this class, a resolution variable defines the number of data points required along each axis for a given chunk. For example, if the resolution is set to 32, each chunk will contain $32^3$data points.

The _Octree_ class performs subdivision based on the player's position relative to the axis-aligned bounding box (AABB) of each leaf node. If the player enters the AABB of a leaf node, the _Octree_ subdivides that node, continuing this process until a predefined MaxDepth is reached. Upon subdivision, the new leaf nodes invoke the _OctreePlanetSpawner_, providing the center and size of their respective AABBs to generate new meshes. The parent node then disables the previously used mesh.

Conversely, when the player exits a leaf node's AABB, the _Octree_ removes the corresponding subdivisions. This process continues until the player is once again located within a valid leaf node, at which point the associated mesh is re-enabled. The only exception is the root node which cannot be removed. 

== Planetary Features
=== Surface Elements #text(red)[William Klar]
In order to enhance the variety of the planet's surface details, it was decided to incorporate elements such as grass, bushes, trees, and oceans. This addition aims to increase the diversity of the planet's landscape. 

The implementation structure for surface details is primarily based on the work presented in the blog _Population of a Large-Scale Terrain_ @surfacedetails1, in which the author classifies surface elements into two distinct categories: *details* and *features*.

The _details_ category includes elements such as grass, which are rendered in close proximity to the player. Precise positioning of these elements is not critical, as minor discrepancies typically go unnoticed due to their abundance and the player's limited focus on individual instances (e.g., the position of a single blade of grass).

In contrast, the _features_ category encompasses elements like trees. These are intended to be visible from a distance and require consistent placement, as irregularities in their positions are more easily perceived and can negatively impact the visual coherence of the scene.

For both categories, it remains important to ensure that the generated elements are aligned with the normal of the underlying mesh. However, due to their differing visual and functional requirements, each category necessitates a distinct implementation approach.

*Surface Details (Grass)* #text(red)[Räknas detta som en till rubriksnivå? även om inte explicit skriven]

To generate surface details, the system first identifies the current chunk associated with the player, along with adjacent chunks within a defined range. It then iterates through all triangles in the mesh data of each relevant chunk. For each triangle, a random point is generated within its bounds using barycentric coordinates, as described in @barycentriccoordiantes1.

Given a triangle with vertices $bold(a)$, $bold(b)$, and $bold(c)$, and two random variables $r_1$ and $r_2$, a random point $bold(d)$ within the triangle can be computed using the following formula @barycentriccoordiantes1:
$ bold(d) = (1 - sqrt(r_1))bold(a) + sqrt(r_1)(1 - r_2) bold(b) + sqrt(r_1) r_2 bold(c) $ 
This method ensures that the point $bold(d)$ lies uniformly within the triangle. To determine the orientation of the surface detail, the normal vector of the triangle is calculated via the cross product of its edge vectors:
$ bold(n) = (bold(b) - bold(a)) crossmark (bold(c) - bold(a)) $
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
Applying a grass shader to the grass blade meshes resulted in enhanced surface detail on the planet, as demonstrated in the final visual output.
#figure(
  image("images/Grass/grass7.png", width: 300pt),
  caption: [Grass on a planet]
)
*Surface Features (Trees)*

Unlike the method utilized for Surface Detail, Surface Features must remain visible at greater viewing distances. Accordingly, features are not limited to rendering within high-resolution chunks proximal to the camera. Additionally, the technique previously employed for Surface Detail—relying on dense mesh triangle data—is incompatible with the current octree-based terrain representation. The hierarchical level-of-detail inherent in octree chunking results in a significantly reduced triangle count at lower resolutions, rendering geometry-dependent techniques ineffective.

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

Although this uniform sampling technique is computationally efficient and straightforward to implement, it tends to produce uneven spatial distributions. Specifically, it may result in clustering of points in some regions and sparse coverage in others—an outcome that is undesirable when attempting to ensure consistent surface feature placement across the planetary surface (see #ref(<randomvspossion>)).
#figure(
  image("SurfaceFeatures/poisson_vs_random.jpg", width: 350pt),
  caption: [Poisson vs random distribution]
)<randomvspossion>

To achieve a more spatially uniform distribution of points, the sampling method was subsequently replaced with Poisson-disc sampling. Unlike uniform sampling, Poisson-disc sampling ensures that each point is separated by a minimum distance $r$ @bridson2007fast, thereby avoiding clustering and producing a more even distribution of samples.
The algorithm used follows the method described in _Fast Poisson Disk Sampling in Arbitrary Dimensions_, which operates as follows @bridson2007fast:

*Step 1. * Initialize a background grid with cell size $frac(r, sqrt(n))$ where $n$ is the dimensionality of the space (in this case, $n = 3$). 

*Step 2. * Randomly select an initial sample within the domain and insert it into both the active list and the background grid.

*Step 3. * While the active list is non-empty:
#list(indent: 2cm,
spacing: 1em, 
[Randomly choose a sample from the active list.],
[Generate up to $k$ candidate points within an annulus of radius $r$ to $2r$ around the chosen sample.],
[For each candidate:
- Reject it if it lies outside the domain or is closer than $r$ to any existing sample (checked efficiently using the grid).
- If it is valid, add it to both the active list and the grid.],
[If none of the $k$ candidates are accepted, remove the sample from the active list.]) 


This revised sampling technique yielded a significantly improved distribution of surface feature placement, resulting in a more visually and functionally satisfactory outcome. 

In order to generate varied environmental features—such as large trees, small trees, and rocks—a method for distributing these elements is required. One effective approach for achieving this distribution is the Alias method @aliasvose. The Alias method enables sampling from a discrete probability distribution in constant time. Given a list of $n$ probabilities, it allows for the selection of an index i, where $1 <= i <= n$, according to the specified distribution. For example consider a case where three types of features are to be placed in a scene: large trees, small trees, and rocks. Suppose the desired probabilities for selecting each feature are as follows:
- Large trees: 50%

- Small trees: 30%

- Rocks: 20%
Using the Alias method, this distribution is encoded into two tables—one for probabilities and one for aliases. At runtime, a uniformly random index is selected, and the final outcome is determined using the stored values, thereby ensuring that features are placed according to the specified probabilities in an efficient manner.



=== Surface Features (Ocean) #text(red)[Anton typ Klar]
At this point, it was possible to create some interesting looking planets. But they where still looking quite barren #text(orange)[and all that vegetation needed some water.]

The ocean layer is implemented as a simple blue sphere that is expanded from the planets center to some configurable radius (typically equal to or slightly less than the planet radius), this sphere will then intersect with the planet terrain and it will appear as if there is an ocean there. However, only having a smooth blue sphere as water would be highly uninteresting. So, to increase the visuals, a water shader was implemented. 

[källan till water-shaders] Research was conducted to find ways of implementing convincing water effects. One method to create moving water was found, [källa] and it entailed scrolling noise textures on the water surface to create what looks like waves. To further increase the visual fidelity, one can interpret these noise textures as normal maps; using the noise to displace the normals of each vertex during lighting calculations (this will result in what looks like "bumps" on the surface), a technique commonly known as bump mapping. This does not create "physical" waves on the water however, it is purely a visual effect, so to create actual moving waves, displacement mapping was also utilized. This technique is similar to bump mapping, but instead of only affecting lightning calculations, the vertices of the mesh are displaced using a height map instead. A combination of both of these methods was used and the result can be seen in @fig:water-ball.

However, one issue with these techniques when applied to a sphere, as opposed to flat ground, is the potential occurrence of seams in the texture and pinching at the poles as can be seen in @fig:water-pinching. The first of these issues was easily solved since Godot has built in support for seamless noise textures. Further research conducted to address the second issue, pinching at the poles, and two potential solutions was found: triplanar uv-mapping or using cube spheres. In the interest of time, this issue was deemed as acceptable as the pinching is often hidden by the planet's terrain and is therefore, unlikely to be seen by most users, so none of these methods were implemented.

One glaring issue was with flickering due to the ocean and the atmospheres clashing, this was resolved by making the atmosphere have a higher render priority.

#align(center,
  grid(
    columns: 2,
    gutter: 55pt,
    grid.cell([
      #figure(
        image("images/Ocean/water_ball.png", width: 160pt, height: 160pt),
        caption: [Ocean layer by itself],
      )<fig:water-ball>
    ]),
    grid.cell([
      #figure(
        image("images/Ocean/water_pinching.png", width: 160pt, height: 160pt),
        caption: [Visible pinching on the ocean's poles]
      )<fig:water-pinching>
    ])
  )
)

The oceans did not yet look cohesive with the rest of the planets, so the next step was to make the water transparent and to color the water based on its depth. This was achieved by using a combination of the depth texture (källa godot docs) to calculate the distance of each pixel to the water surface, and the screen texture (källa godot docs) to blend the planet's surface color with the water color. The water color was also divided up into two separate colors, one brighter color for shallow parts and a darker color for deeper parts. Using the Godot built-in function for blending two colors, the final per pixel water color was calculated as seen below:

```cs
vec3 water_depth_color = mix(deep_color, shallow_color, smooth_shallow_depth);
vec3 transparent_water_depth_color = mix(screen * smooth_shallow_depth, water_depth_color, transparency_blend);
```

Finally, the color around the edges of the ocean (where the ocean surface meets the terrain), was made to be white to appear as foam. This was done by blending the previously calculated color with a foam color:
```cs
vec3 transparent_water_depth_color_with_foam = mix(transparent_water_depth_color, foam_color, smooth_foam_depth);
```

Put together, the ocean added a large improvement to the visuals of the planets, the result can be seen in @fig:ocean-planet.

#figure(
  image("images/Ocean/water_planet1.png", width: 160pt, height: 160pt),
  caption: [Planet with ocean and water shader],
)<fig:ocean-planet>


=== Atmospheres #text(red)[ERIK KLAR]
Before starting development on the atmospheres, research was made to try to find existing solutions and inspiration for how it could be done. Two videos that explained two different approaches to the problem were found. One created by Martin Donald that explains a simpler approach #cite(<martin-donald-atmosphere>), and one made by Sebastian Lague that explains a more complex but realistic approach #cite(<sebastian-lague-atmosphere>). The simpler version was implemented at first to quickly get something working, followed by the realistic one, which was the ultimate goal to replicate. Both of the solutions tested were implemented as post-processing shaders applied to a cube with flipped faces to get the desired effect.

The first iteration of the atmosphere was based purely on ray sphere intersections that could generate a transparent colored sphere around the planet (see #ref(<ray-sphere-atmosphere>)). This was, as mentioned, the simplest version of the atmosphere as it did not account for the sun's position, which meant that it remained a single uniform color regardless of how the sun shone on the planets. Unsuccessful attempts at shading the atmosphere were made by calculating the dot product between each vertex normal and the sun ray directions. The color of the atmosphere were then supposed to change based on the produced value. Due to issues with fetching the correct sun position, and issues with different coordinate spaces, the attempt was unsuccessful. As a result, development shifted toward a more advanced solution in the second iteration.

#figure(
  image("images/Atmosphere/basic_atmosphere.png", width: 50%),
  caption: [Implementation of a basic atmosphere]
)<ray-sphere-atmosphere>
The second iteration intended to generate a more physically realistic atmosphere. As mentioned earlier, the inspiration for this atmosphere was found in Sebastian Lague's video, in which he created a planetary atmosphere based on pure Rayleigh scattering. Rayleigh scattering is a physical phenomenon which describes how light is affected by particles much smaller than the wavelength of the light #cite(<RayleighScattering>). This would allow simulating atmospheric light scattering, density falloff with altitude, and visually accurate effects such as sunsets when observed from a planet’s surface.

The color is based on a three dimensional vector containing three wavelengths, corresponding to different parts of the visible light spectrum. However, basing the atmosphere color on just Rayleigh scattering limited the achievable color options. For example, creating a naturally red, Mars-like atmosphere was not possible through this method alone. To address this, one of the wavelength values was set significantly lower than the others, amplifying the scattering of that particular wavelength and thereby altering the perceived color. Finally, to get reasonable colors, some preset wavelength vectors were made. These were chosen at random as the planets were generated (see #ref(<AtmosphereColors>)). 
#figure(
    grid(
        columns: (auto, auto),
        rows:    (auto, auto),
        gutter: 1em,
        column-gutter: -120pt,
        [ #image("images/Atmosphere/blue_atmo.png",   width: 45%) a) Blue atmosphere #image("images/Atmosphere/purple_atmo.png", width: 45%) c) Purple atmosphere],
        [ #image("images/Atmosphere/orange_atmo.png", width: 45%) b) Orange Atmosphere
        #image("images/Atmosphere/green_atmo.png", width: 45%) d) Green atmosphere]
    ),
    caption: [Main colors of the different atmosphere variations.]
) <AtmosphereColors>

The initial implementation of atmospheric rendering significantly impacted performance due to the computational cost of light scattering calculations. Options to solve this were discussed: offloading calculations to a compute shader, implementing multiple levels of detail (LOD) for atmospheres, or maintaining the current method while integrating an LOD system to reduce rendering complexity at greater distances. The latter was selected for its simplicity. This approach dynamically adjusts the number of scattering and optical depth sampling points based on the player’s distance from a planet. Initially, both values were set to 30, which resulted in severe performance drops when near planets. After testing alternative configurations, a value of 10 for both parameters achieved a more optimal balance between visual aesthetics and performance. Benchmarking in a test scene with a single planet showed an increase in average frame rate from 39.2 FPS (with 30 sampling points) to 59.9 FPS (with 10 sampling points).


== Player Controller #text(red)[ERIK KLAR] <player-controls-ref>
The initial player controller was implemented as a simple flying camera, allowing free movement and rotation for exploring the galaxy and star systems. Collision detection and planetary landings were not considered necessary at this stage.

Aa landing was considered a ‘should-have’ feature development began after terrain generation was implemented. Key subcomponents included simulating planetary gravity, enabling surface movement with proper rotation, and allowing jumping.

Planetary gravity fields were implemented using an Area3D node with a spherical collision shape (see #ref(<Area3D>)). This node allows any physics body that enters it's collision shape to inherit the gravity direction and strength set by the Area3D. The gravity direction is calculated by subtracting the planet's world position with the player's world position. This is updated each physics process to allow the direction to always point towards the planet center. 
#figure(
  image("images/Planet/Area3D.png", width: 50%),
  caption: [Area3D node with collision shape around a planet]
)<Area3D>
To facilitate exploration and simulate orbital behavior, the player inherits a planet’s total velocity upon entering its gravitational field, ensuring they remain in orbit with actively moving. The final step was implementing surface landing and movement mechanics.

Rotating the player while moving on the planetary surfaces was achieved by linearly interpolating the player’s basis toward a target basis with its ‘up’ vector aligned against the gravity vector. Initially, this caused conflicts with camera movement, as the camera rotated the player around its local Y-axis while the movement system applied rotation relative to a global axis. This was resolved by ensuring that the camera system rotated the player around it's global Y-axis instead. Finally the ability to jump was implemented by adding an impulse along the opposite direction of the gravity vector.

#text(red)[Behövs detta/ är det intressant med vilka problem som finns? VI kanske kan skita i det?!????]
During testing, several issues emerged. The player can occasionally fall off the planets at high velocities, this was solved by lowering the base speed to a small value. A possibility considered was to adapt the base speed depending on planet radius, but was not implemented as it was deemed unnecessary. Another issue is that the player can sometimes start bouncing uncontrollably. A possible explanation to this is the unevenness of the generated terrain. If the player is moving at high speeds they could possibly bounce off of any small bump they encounter. To counteract this, a downward raycast was added beneath the player’s collision shape, supplementing the default collision system in cases where it falters. A final edge case issue involved overlapping gravitational fields from adjacent planets, occasionally pulling the player toward a second planet. Due to the rarity of this occurrence, it was not prioritized for resolution.

== Physics Engine #text(red)[JONATAN TYP KLAR] <physics-engine-ref>
Simulating the gravitational interactions within a galaxy, containing potentially thousands or millions of stars and planets, presents a significant computational challenge known as the N-body problem. The goal is to calculate the net gravitational force acting on each body at discrete time steps and use this information to update their positions and velocities over time. This section details the progression of methods implemented to tackle this problem within our project, moving from a simple baseline to an optimized approximation algorithm, and discusses the performance analysis that guided these choices.

=== Direct Summation #text(red)[Jonatan Klar]
The most straightforward approach to solving the N-body problem is the direct summation method. This technique relies directly on Newton's Law of Universal Gravitation #text(red)[CITE NEWTON?], calculating the gravitational force between every pair of particles in the system.

The force $arrow(F)_12$ exerted on particle 1 by particle 2 is given by:
$
  arrow(F)_12 = G (m_1 m_2)/(|arrow(r)_12|^3) vec(r)_12 quad "where" quad arrow(r)_12 = arrow(r)_2 - arrow(r)_1
$

The total acceleration $arrow(a)_i$ on particle $i$ is the sum of accelerations caused by all other particles $j != i$:
$
  arrow(a)_i = sum_(j != i) G m_j/(|arrow(r)_(i j)|^3) arrow(r)_(i j)
$

#figure(
  image("pairwise.png", width: 50%),
  caption: [
    Diagram illustrating pairwise force calculation between 5 bodies
  ]
)


This requires $N(N−1)/2$ pairwise calculations per time step, resulting in a computational complexity of $O(N^2)$. Our implementation (`rust_gdext/src/physics/gravity/direct_summation.rs`) uses nested loops as illustrated in the pseudocode below:


#box[
#algorithm({
  import algorithmic: *
  Function("Direct-Summation-Gravity", args: ("G", "particles"), {
    Cmt[Get the number of particles]
    Assign[$n$][#FnI[length][particles]]

    State[]
    Cmt[Initialize acceleration array]
    Assign[acc][#FnI[array][#FnI[Vec3][0, 0, 0]\; $n$]]

    State[]
    Cmt[For each particle $i$, calculate acceleration from each particle $j != i$]
    For(cond: [$i$ *in* $0..n$], {
      For(cond: [$j != i$ *in* $0..n$], {

        Assign[$vec(r)_(i j)$][particles[j].*position* - particles[i].*position*]
        Assign[$m_j$][particles[j].*mass*]
        
        State[]
        Cmt[Note: Softening factor $epsilon$ omitted for brevity, but used in implementation]
        If(cond: [$|vec(r)_(i j)|> 0$], {
          Assign[$vec(a)_(i j)$][$G m_j/(|vec(r)_(i j)|^3) vec(r)_(i j)$]
          Assign[acc[i]][acc[i] $+ vec(a)_(i j)$]
        })      
      })
    })

    Return[acc]
  })
})
]

While simple and accurate, the $O(N^2)$ complexity makes direct summation computationally prohibitive for large N within our target performance goals, necessitating an approximation method.

=== Barnes-Hut Approximation #text(red)[Jonatan Klar]
To efficiently simulate large numbers of bodies, we implemented the Barnes-Hut algorithm @Barnes_Hut_1986, which reduces the computational complexity to $O(N "log"N)$. The core idea is to use an *octree* (a hierarchical spatial partitioning structure, described in @B-octree) to group distant particles together. The gravitational influence of these distant groups is then approximated by treating the group as a single point mass located at its center of mass (CoM). This approximation leverages Newton's shell theorem and is effective when the distance to the group is large compared to the group's size.

A crucial aspect of dynamic N-body simulations is that particle positions change continuously. This means the spatial hierarchy represented by the octree becomes outdated quickly and must be reconstructed frequently, typically time step (physics frame). This frequent rebuild demands a highly efficient octree construction algorithm, motivating our choice of a *Morton-code-based linear octree* #text(red)[citation] (`rust_gdext/src/octree/morton_based.rs`). This approach offers several advantages for rapid reconstruction:

1.  *Parallelizable Steps:* Key phases of the construction — calculating Morton codes and sorting particles — are trivial to parallelize.
2.  *Cache Efficiency:* Processing spatially local data sequentially after sorting can lead to better CPU cache utilization compared to pointer-chasing in traditional octree implementations.
3.  *Efficient Partitioning:* The sorted order allows for fast partitioning of particles into child nodes using binary search rather than geometric tests.

The construction process proceeds as follows:
- *Morton Codes:* A 64-bit Morton code is calculated for each particle using the `encode` function. This function maps a particle's 3D position within the global simulation bounds to a 1D integer by interleaving the bits of its scaled coordinates [CITE MORTON]. This mapping largely preserves spatial locality – nearby particles tend to have numerically close Morton codes. This encoding step is parallelized using `rayon` #text(red)[CITE RAYON] for particle counts larger than `PARALLEL_ENCODE_THRESHOLD`, a threshold determined via benchmarking (see @physics-benchmarking-ref).


#grid(
  columns: (1fr, 1fr),

    figure(
      image("morton_z_curve_2d.png", width: 94%),
      caption: [2D version of Morton codes, showing how they preserve space locality. Nomen4Omen, CC BY-SA 4.0]
    ),
    
    figure(
      image("morton_3d.png"),
      caption: [How points are ordered in 3D when sorted using Morton codes. Robert Dickau, CC BY-SA 3.0]
    )

)

- *Sorting:* The particles (represented by ```rs MortonEncodedItem<usize>``` containing the code and original index) are then sorted based solely on their Morton codes. This crucial step clusters spatially adjacent particles together in a linear array. This sorting is performed in parallel using `rayon::par_sort_unstable`, providing significant speedup.

- *Tree Construction (`build_recursive`):* An explicit tree structure (`Vec<Node>`) is built recursively from the sorted particle list.
    - The function operates on a range (`body_range`) within the sorted list.
    - Leaf nodes are created if the range contains only one particle or the maximum depth (`MAX_DEPTH`) is reached. `GravityData` (mass, CoM) is computed from the particles in the range.
    - For internal nodes, the algorithm partitions the `body_range` into 8 sub-ranges corresponding to the child octants. This is done efficiently by `find_octant_split`, which performs a binary search (`partition_point`) on the sorted Morton codes, checking the relevant 3 bits at the current depth (`get_octant_index_at_depth`) to find the split points.
    - Recursive calls are made for each non-empty child sub-range.
    - Parent nodes aggregate `GravityData` from their completed children.
    - In our current implementation, this recursive building phase itself runs *sequentially*, although the potential for parallelizing it exists.

The `Node` struct stores the necessary information for the algorithm:
#box[
```rs
  struct Node {
    bounds: BoundingBox,
    children: [Option<NonZeroUsize>; 8],
    body_range: Range<usize>, // Range in sorted_indices
    depth: u32,
    data: GravityData, // Mass, CoM
  }
```
]

Once the octree is built for the current time step, the force (acceleration) calculation proceeds by traversing this tree for each particle $i$. The traversal starts at the root and uses the _Multipole Acceptance Criterion (MAC)_ to decide whether to approximate or recurse:

1.  Calculate distance $d$ from particle $i$ to the node $n$'s CoM.
2.  Get the node's size $s$ (bounding box width).
3.  If $s^2 < theta^2 d^2$ (where $theta$ is a threshold parameter), the node is far enough away. Its gravitational effect is approximated using the node's total mass and CoM (`node.data`), and the traversal down this branch stops.
4.  If the node is too close:
    * If it's an internal node, recursively traverse its non-empty children.
    * If it's a leaf node, perform direct summation between particle $i$ and all other particles $j != i$ within that leaf's `body_range`, accessing original particle data via the `sorted_indices` map and `data_ref`. A softening factor $epsilon$ is used to avoid singularities ($d^2$ replaced by $d^2+epsilon^2$).
    
This traversal logic is implemented in `calculate_accel_recursive`. The crucial optimization here is that the force calculation for *each* particle $i$ is an independent tree traversal. Therefore, these traversals are trivially parallelized using `rayon`:

#box[
```rs
// From NBodyGravityCalculator impl in src/physics/gravity/controller.rs
accelerations
    .par_iter_mut() // parallel iterator through rayon
    .enumerate()
    // Calculate acceleration for particle `i` by traversing the octree
    .for_each(|(i, acc)| {
         // Starts the recursive traversal
         *acc = self.calculate_accel_on_particle(i); 
    });
```
]

#text(red)[Diagram illustrating the BH traversal for one particle (MAC pass/fail).]

=== Integration #text(red)[Jonatan Klar]
Finally, the calculated accelerations (whether from Direct Summation or Barnes-Hut) are used to advance the simulation state via numerical integration. Our implementation uses the Forward Euler method @euler1768integral (specifically, symplectic Euler, which offers better long-term stability for orbital mechanics @brorson_symplectic_integrators)  within the `step_time` function (`controller.rs`) to update velocities and positions based on the accelerations computed in parallel for the current time step $Delta t$:

$v_"new" = v_"old" + arrow(a) dot Delta t quad $ (```rs body.vel += acc * delta```)

$p_"new" = p_"old" + arrow(v)_"new" dot Delta t quad $ (```rs body.pos += body.vel * delta```)

This combination - parallelized efficient octree construction via Morton codes, $O(N "log"N)$ force calculation with parallel traversals, and simple Euler integration - allows the simulation of large-scale galactic systems.

=== Performance Benchmarking and Threshold Tuning #text(red)[Jonatan Klar] <physics-benchmarking-ref>
To guide optimization efforts and make informed decisions about algorithm choices and parallelization strategies, rigorous performance benchmarking was conducted on the core components of the physics engine. We utilized the `criterion` @criterion.rs Rust library, a powerful statistical benchmarking harness. `Criterion` provides several advantages over simple timing loops, including running benchmarks multiple times to gather statistically significant data, detecting performance regressions between code versions, and generating detailed reports, making it invaluable for performance analysis. Benchmarks were defined in `rust_gdext/benches/gravity_bench.rs` and executed via `cargo bench`.

The benchmark suite was designed to measure the performance of critical functions under varying workloads, primarily different numbers of simulated bodies (`N`). Key benchmark groups included `compute_accelerations` (comparing Direct Summation vs. Barnes-Hut) and specific parts of the Morton-based octree construction like `morton_encode` and `morton_sort`. Test data (`SimulatedBody` instances) was generated consistently using helper functions like `create_bench_bodies` to ensure repeatable results across runs.

The insights gained from these benchmarks directly influenced several implementation details, particularly the selection of performance thresholds:

1.  *Parallel Morton Encoding Threshold (`PARALLEL_ENCODE_THRESHOLD`):*
    The `morton_encode` benchmark compared the performance of calculating Morton codes sequentially versus in parallel using `rayon`. As shown by the benchmark results (#text(red)[REF APPENDIX]), while parallelization offers benefits for large datasets, the overhead associated with thread management and work distribution makes the sequential version faster for smaller numbers of particles. The parallel version only overtakes the sequential one after a certain crossover point. Based on these measurements, the threshold `PARALLEL_ENCODE_THRESHOLD = 3000` was selected in the Morton-based octree. Below this number of bodies, Morton codes are calculated sequentially; at or above this threshold, the parallel `rayon::par_iter` implementation is used.

2.  *Direct Summation vs. Barnes-Hut Threshold:*
    The `compute_accelerations` benchmark directly compared the $O(N^2)$ `DirectSummation` method against the $O(N "log"N)$ `MortonBasedOctree` (Barnes-Hut) implementation. While Barnes-Hut has better asymptotic complexity, it incurs a higher constant overhead (tree build, traversal). Direct summation has low overhead but scales quadratically. The benchmark results (#text(red)[REF]) confirmed this trade-off, showing that direct summation was faster below approximately $N=100$. Consequently, a dynamic switching mechanism was implemented in the physics controller (`rust_gdext/src/physics/gravity/controller.rs`):
    #box[
    ```rs
    let accelerations = if bodies_sim.len() < 100 {
        // Use sequential direct summation for small N
        DirectSummation::calculate_accelerations::<false>(/*...*/)
    } else {
        // Use parallel Barnes-Hut for larger N
        MortonBasedOctree::calculate_accelerations::<true>(/*...*/)
    };
    ```
    ]
    
    This ensures the simulation adaptively uses the most efficient algorithm based on the current number of bodies.

In summary, using `criterion` for systematic benchmarking was crucial for optimizing the physics engine. It provided the quantitative data needed to justify algorithmic choices and fine-tune parameters like parallelization and algorithm-switching thresholds, leading to a more performant and scalable simulation.

=== Trajectory Simulation and Visualization <P-trajectories>

To understand orbital dynamics and aid in system design, a trajectory simulation system was implemented (`rust_gdext/src/physics/gravity/trajectories.rs`) to predict and visualize the future paths of celestial bodies. This system runs a separate N-body simulation for a configurable number of future steps (`simulation_steps`) and time increment (`simulation_step_delta`), using the same core physics logic (Direct Summation or Barnes-Hut) and semi-implicit Euler integration as the main simulation (`GravityController::step_time`). The resulting sequence of future positions for each body is stored in a `Trajectory` struct and can be rendered as colored line strip meshes (`MeshInstance3D`) in Godot. Trajectories can also be calculated relative to a central body.

Given the computational cost, especially for many steps or bodies, trajectory calculations are offloaded to a background thread managed by `TrajectoryWorker` (an instance of the generic `Worker` found in `rust_gdext/src/worker.rs`). The main thread sends `TrajectoryCommand::Calculate` messages with `SimulationInfo` to this worker and retrieves results asynchronously using a queue-and-poll mechanism (`queue_simulate_trajectories`, `poll_trajectory_results`). This prevents the main game loop from freezing during intensive calculations. The worker is designed to process the latest request if multiple are queued, ensuring responsiveness.

The accuracy of these predicted trajectories is subject to the same numerical errors as the primary Euler integration, accumulating with the number of steps; smaller `simulation_step_delta` improves accuracy at the cost of computation. However, it was observed that less precise trajectories (larger `delta`) tended to overestimate instability, providing a useful heuristic: if a system appeared stable with coarse predictions, it was generally stable in practice.

This trajectory visualization proved invaluable for the `System Generation` process (@system-gen-ref), allowing for iterative tuning of orbital parameters to achieve stable or aesthetically desirable configurations. It served as a key diagnostic tool for debugging physics and visually confirming the immediate future dynamics of generated solar systems.


== System Generation #text(red)[Paul klar, kan behöva renskrivas lite] <system-gen-ref>
System generation means creating a system from a seed such that the system is stable and has certain aesthetics. The generation includes placement of planets and moons such that they are in a stable orbit. It's also responsible for creating the seeds of the individual bodies in the system.

The generation of systems has to make a tradeoff between realism and aesthetics, while keeping the system stable. Creating stable system with realistic distance/planet-size is easy but comes at a cost of aesthetics. With distances as large as they are in real life the planets become really hard to see, which doesn't make for a good experience.

Creating smaller distances and larger planets helps with aesthetics but can come at a cost of stability, specifically for moons. Having larger planets means that the moons will have to be orbit further. This increases the risk of instability due to the moons interacting with other planets.

This investigate if a certain setup resulted in a stable or unstable system it was helpful to see the trajectories of the planets. The trajectories were calculated by calculating the position of the planets a certain number of steps $n$ in the future where each step would go $Delta t$ time in the future. To see further into the future we would increase $n$. To see a more accurate trajectory we would lower $Delta t$. The faster the planets are moving the lower $Delta t$ is needed to get accurate enough trajectories. One helpful thing to note is that in almost all cases a less accurate trajectorie *overestimates* the risk of instability/chaos. Meaning if a system looked stable according to the trajectories with low granularity (high $Delta t$) we could be confident that the system is actually stable. (as far into the future as the trajectories showed)


=== General Flow #text(red)[PAUL KLAR]
The input seed for the system gets added as the seed to a random number generator. This ensures that it produces the same random numbers each time the system gets generated. For each attribute of the system a value for that attribute gets randomized within an interval like so:

```ts
func generateSystemDataFromSeed(s: int):
	var r = RandomNumberGenerator.new()
	r.seed = s

func randomPlanetMass(r):
	return r.randf_range(MIN_PLANET_MASS, MAX_PLANET_MASS)
```

=== Star #text(red)[PAUL KLAR]
The only randomized attribute of the star is it's color. Which gets picked randomly out of a list. 
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

The mass of star is constant across all systems but is something which can easily be changed if needed. The star gets placed at coordinate (0,0,0) and always has the same radius. Again, this is something can be changed to scale with mass if we would like to. It's worth to note however that one can only notice a change in size *relative* to another object. Same with the mass. It has been largely unnecessary to modify the stars mass/radius as the stars don't interect and can't be seen side by side. So modifying the look of the solar system only required modifying the planets and moons.

=== Planets #text(red)[PAUL KLAR]
 The system randomizes how many planets it should have.
```py
  var n = r.randi_range(MIN_NUMBER_OF_PLANETS, MAX_NUMBER_OF_PLANETS)
```

The planets orbit radiuses were at first scaled linearly, with the same difference in orbitradius between two adjacent planets.

```py
var orbit_radius = SUN.radius + BASE_DISTANCE_FROM_SUN
	for i in range(n):
		var orbit_increase = DISTANCE_BETWEEN_PLANETS
		orbit_radius += orbit_increase #Linear;
```

When adding moons and pushing the planets closer together the outer planets became unstable. To mitigate this the orbitradius was changed away from scaling linearly to some scaling were that distance between planets increased the further out in the system. We found that linearithmic scaling worked the best. With linearithmic scaling the planets are close enough to be visible throughout the system, but far enough away from eachother so that moons can orbit at a distance and be decently sized with destabilizing. 

Taking inspiration from how the planets are organized in our real solar system helped as the planets further out have larger distances between them. But they're divided into the "inner planets" and the "outer planets" by the asteroid belt. Both having a roughly linear or linearithmic scaling just with different slopes.@planetary-fact-sheet

```py
var orbit_radius = SUN.radius + BASE_DISTANCE_FROM_SUN
	for i in range(n):
		var orbit_increase = DISTANCE_BETWEEN_PLANETS
		orbit_radius += log(i + 1) * orbit_increase #Linearithmic;
```

In order for a planet to stay at the same distance from its star, it gets an initial velocity perpendicular to the vector pointing towards the star. The velocity $v$ scales with the stars mass $m$, the gravitational constant $G$ and inversely with the orbit radius $r$.

$ |v| = sqrt(G dot m/ r) $

As the velocity scales with $G$ it's easy to modify the orbit speeds of planets within systems. Simply increasing G will create faster orbits in systems. One could also modify $m$ to change the speed of the orbits. So to create more variation between the systems we could change the orbit speeds by modifying $m$.

Creating elliptical orbits is possible but comes with a risk as the distance between planets is harder to guarantee, causing chaotic systems more easily. 

Besides the placements of planets the system also randomizes orbitangle (what angle around the star it should start at), mass and size (radius) of planets. They mass and size are randomized independent of each other, when in reality the mass scales with the radius cubed. The radius gets used when generating the planet texture@planet-gen-ref and placing moons.

=== Moons #text(red)[William (generated, textured) och PAUL KLAR (system)]

The moons were placed around planets in a similar way to how planets were placed around the star. The number of moons increased by 1 starting from the fourth planet, with the 3 innermost planets having zero moons.

The orbitradius of moons around their planet increased linearly, with the moons being placed at even steps around their planet. The stepsize/slope wasn't fixed but instead based on the difference in orbitradius between the planet and the previous planet. The stepsize, _distance_between_moons_, is calculated as the orbit difference divided by a constant _MOON_ORBIT_RATIO_PLANET_DISTANCE_ which is set to somewhere in the range 40-100. Higher ratio means the moons are closer to the planet and closer together.

```py
  var moons_count = max(2, i) - 2
  var distance_between_moons = orbit_increase / MOON_ORBIT_RATIO_PLANET_DISTANCE
  for m in range(moons_count):
    var moon_orbit_radius = planet_data.radius + (m + 1) * distance_between_moons
```

The velocity is calculated in the same way as with the velocity of the planet. But the mass $m$ is instead the mass of the planet the moons orbit.
 $ |v| = sqrt(G dot m/ r) $

 Orbitangle is calculated exactly the same as with planets. Mass is not randomized but instead directly calculated as 1/10 000 of the planets nass. The radius is randomized but within the range of $r/10 -r/5$ where $r$ is the planets radius.
 
 ```py
 var moon_radius = r.randf_range(planet_data.radius / 10, planet_data.radius / 5)
	var orbit_angle = randomOrbitAngle(r)
 var moon_mass = planet_data	.mass * 0.0001
 ```


== Galaxy #text(red)[Jacob, ANTON KLAR] #text(blue)[blå är förslag på ny text],#text(red)[ röd är kommentarer],#text(orange)[ text markerad med orange är din text och kommentaren som kommer efteråt syftar till den oranga texten. Tex. orange + blå => ersätta orange text med blå], #text(purple)[lila betyder Ta bort]
A galaxy is a massive collection of stars, gas and dust, ranging in diameters of 1500 to 300,000 light-years@galaxy-term. In the context of this project, the galaxy represents the largest scale of the simulation -- a vast space populated by procedurally placed stars.

#text(red)[detta stycke hade nog passat mer i resultat? passar inte direkt i processen för du pratar om hur slutversionen fungerar ]A key feature of the Galaxy's implementation is deterministic generation, or "seeded" generation. This approach allows for the "random" values produced by a random number generator to be predetermined based on an initial seed. This is desirable since a goal of this project is to ensure reproducible and consistent generation.

#text(red)[samma här, nämner också seed mycket men inte förklarat vad det är ]All iterations of the galaxy utilizes an arbitrary integer seed to influence the generation of stars, with the same input seed always yielding the same galaxy. The term "randomly" is used loosely, as it refers to this #text(red)[controlled process, lite otydligt vilken process den menar ]controlled process. #text(blue)[The term "random" is used loosely, as it refers to the described seeding process of always generating the same star configuration with the same seed.] #text(red)[kanske något sånt istället?]

The following sections introduce the various iterations the galaxy underwent during development, #text(orange)[each exploring new, or refined approaches to star interaction and distribution.] #text(red)[kan möjligtvis tas bort]

=== Star field #text(red)[Jacob, ANTON KLAR] <star-field-ref>
The first version #text(blue)[of the galaxy] was a three-dimensional star field#text(blue)[,] #text(red)[vad är en star field?] as can be seen in @star-field-img. Points were sampled randomly within a cube to determine the location of #text(orange)[star placement] #text(blue)[each star]. #text(orange)[The stars #text(purple)[are] made up out of a single circular mesh, @star-img.] #text(blue)[The stars were constructed using a single circular mesh, as displayed in @star-field-img] This #text(orange)[iteration] #text(blue)[version] of the galaxy was finite in scale.

#align(center,
  grid(
    columns: 2,
    gutter: 10pt,
    grid.cell([
      #figure(
        image("images/Galaxy/star_field.PNG", width: 220pt, height: 160pt),
        caption: [Star field],
      ) <star-field-img>
    ]),
    grid.cell([
      #figure(
        image("images/Galaxy/star.PNG", width: 220pt, height: 160pt),
        caption: [Star mesh]
      ) <star-img>
    ])
  )
)

=== Disc galaxy #text(red)[Jacob, ANTON KLAR] <disc-galaxy-ref>
Shortly thereafter, a version to imitate a disc galaxy formation #text(red)[vad är en disc galaxy formation?] was created. #text(orange)[The implementation was based on a slightly modified star field from @star-field-ref.] #text(red)[kan nog tas bort] Rather than sampling random points from within a cube, #text(blue)[as done previously,] they were #text(blue)[now] sampled from within a sphere. Depending on #text(orange)[each] #text(blue)[the] sample#text(blue)[']s height #text(blue)[within the sphere] #text(purple)[position], #text(orange)[the likelihood of a star being placed decreased from further away from the center of the galaxy] #text(blue)[the likelihood of a star being placed decreased the farther it was from the center of the galaxy] #text(red)[hur har height position något att göra med sänkt chans att en stjärna utplaceras?]. #text(orange)[This combined] #text(blue)[The combination of these things]#text(purple)[,] resulted in a disc shape as well as a concentration of stars #text(orange)[towards] the center, as can be seen in @disc-galaxy-img.

#figure(
  image("images/Galaxy/disc_galaxy.PNG", width: 60%),
  caption: [Disc galaxy],
) <disc-galaxy-img>

=== Skybox #text(red)[Jacob, ANTON KLAR] #text(red)[- KAN TAS BORT OM VI BEHÖVER MINSKA ORD - bra att nämna någonstans åtminstånde tycker jag även om man tar bort denna underrubriken]
A traditional skybox was created in Blender @blender @blender-youtube to serve as a background, #text(orange)[primarily to be used for the half-time presentation of the project when visiting a solar system] #text(red)[tas bort?]. Unlike the procedurally generated star fields, the skybox does not contain actual 3D stars. Instead, it consists of a pre-rendered image designed to imitate a galaxy of stars. As shown in @skybox-testing-img.
#text(red)[används denna i slutprodukten? Räcker kanske att bara nämna att den är gjord i blender och att det är en pre-rendered image]

#figure(
  image("images/Galaxy/skybox_testing_environment.PNG", width: 60%),
  caption: [Skybox testing environment],
) <skybox-testing-img>

#text(orange)[As mentioned, this approach was primarily used for presentation] #text(red)[upprepning, det sas typ för 2 meningar sen] but also testing purposes. Since the final aim is a star backdrop composed of actual stars that can be explored, as in the other galaxy iterations. So this implementation won't necessarily be used in the future.

=== Infinite galaxy #text(red)[Jacob klar] <infinite-galaxy-ref>
This version is based on the original star field concept from @star-field-ref, this time, extending infinitely in all directions rather than being limited to a confined structure. Stars are distributed procedurally using a seeded random generator. The result can be seen in @infinite-galaxy-img.

// källor för noise o liknande? antagligen så har vi förstås beskrivit det i ett "bakgrunds"-kapitel.
Additionally, star placement is now influenced by sampling a noise texture, which can help create formations of stars rather than purely random distributions. These star arrangements can result in regions of higher or lower concentrations of stars, making the galaxy more varied.

#figure(
  image("images/Galaxy/infinite_galaxy.PNG", width: 70%),
  caption: [Infinite galaxy],
) <infinite-galaxy-img>

The galaxy is also chunked, allowing generation of stars in the player's closest vicinity, while culling chunks that are further away. This allows the galaxy to be infinitely explorable, with new chunks of stars generating as the player moves through space. An example of a "Star chunk" can be seen in @star-chunk-img.

#figure(
  image("images/Galaxy/star_chunk.PNG", width: 70%),
  caption: [Star chunk],
) <star-chunk-img>


=== Finite physics-based galaxy #text(red)[Jacob klar]

//-Changed to multimesh here as well.
//-Star finder refactored to work with octree's, as well as for moving stars (maybe)

An Infinite galaxy is a compelling concept, but applying physics to stars of an ever-expanding galaxy is not doable. Since such galaxies are infinitely vast, there is not any fixed point of reference making any attempt at global physics calculations not make sense.

With great advancements in the physics engine from @physics-engine-ref, an attempt to simulate physics of a finite disc-shaped galaxy was performed -- no longer confined to the bounds of the solar system.

To test this idea the disc galaxy implementation from @disc-galaxy-ref was revisited and repurposed. It was retrofitted with new stars containing mass and velocity, to interact with each other through the physics engine. The resulting galaxy can be seen as follows:

#align(center,
  grid(
    columns: 2,
    gutter: 10pt,
    grid.cell([
      #figure(
        image("images/Galaxy/physics_galaxy_before.PNG", width: 220pt, height: 160pt),
        caption: [Physics galaxy - Before],
      )
    ]),
    grid.cell([
      #figure(
        image("images/Galaxy/physics_galaxy_after.PNG", width: 220pt, height: 160pt),
        caption: [Physics galaxy - After]
      )
    ])
  )
)

Since all masses are equal, and no initial velocity is set, the galaxy does not remain stable. However, it still demonstrates the potential of simulating a galaxy using the physics engine. With 10,000 stars in this initial setup, the performance impact remained minimal. Given more time, this would have been explored further.

== Galaxy map #text(red)[Jacob klar]<galaxy-map-ref>
The Galaxy Map serves as the connection of all previous components of the project, and combining them into a unified experience. It accumulates all prior work of the Infinite galaxy (@infinite-galaxy-ref), as well as connects with the implementations of planets (@planet-gen-ref), solar systems (@system-gen-ref), physics (@physics-engine-ref), player controls (@player-controls-ref), and more.

=== Selectable stars #text(red)[Jacob klar] <selectable-star-ref>
To enable interaction with individual stars, a new type of selectable star was implemented. Players can now hover over a star with the mouse cursor and click to select it. This was achieved by adding a spherical collision shape to the star object, which detects mouse input events within the collider. This star, and its collider, is shown in @selectable-star-img.

#figure(
  image("images/Galaxy/selectable_star.PNG", width: 60%),
  caption: [Selectable star],
) <selectable-star-img>

Infinite galaxy (@infinite-galaxy-ref) was developed to allow for distribution of any Godot Node3D scene passed to it, and not only the original star implementation. With this, the star was exchanged for the new selectable star without issues.

To indicate that a star has been selected, the star's location in space is highlighted, together with a distance measured in "Light years" (LYs). This can be seen in the center of @galaxy-map-img. In addition, the coordinates and unique seed of the star is displayed in the bottom-right corner.

=== Navigation #text(red)[Jacob klar] <galaxy-map-navigation-ref>
Two distinct modes of transportation have been implemented for navigating the galaxy map.

1. Manual movement: The player can freely move around using the same player controls introduced in @player-controls-ref.
2. Fast travel: Once a star is selected, press the "->"-button in the bottom-right of @galaxy-map-img. This moves the player rapidly towards it, stopping a short distance away.

To explore the solar systems themselves, the "Explore"-button in the bottom-right of @galaxy-map-img, can be used to enter the star/solar system currently selected. When pressed, a solar system is generated based on the selected star's seed and transitions the player into it. This system exists in a separate scene from the Galaxy Map.

/// även nämna hur när man går iväg från planeter inne i solsystem så går man ut o tillbaks till galaxy map?
/// samt navigeringen (bara flyg atm) runt planeterna i solsystemen.

=== Seed #text(red)[Jacob klar]<seed-ref>
The galaxy utilizes a unique "Galaxy Seed", the same used in @infinite-galaxy-ref, to deterministically generate the placement of stars. With the implementation of explorable solar systems, a need arose to generate new seeds for each system. Were they to utilize the same seed, all solar systems would be identical.

To address this, a custom hash function was developed, allowing for the generation of unique, deterministic, seeds for each star. This function takes into account both the initial Galaxy Seed, and the X, Y, and Z coordinates of a star's position, to produce a star-specific seed. This new seed is then propagated into the stars generation algorithm, which results in unique solar systems while still ensuring deterministic consistency.

// källor för hash-functions?
// implementationsdetaljer? nja. kanske en fin bild på ngt vis som bara visar hur nya seeds genereras?

#figure(
  image("images/Galaxy/galaxy_map.PNG", width: 80%),
  caption: [Galaxy map],
) <galaxy-map-img>


=== Multi-Meshed Stars & Star Finder #text(red)[Jacob klar]
As the scale of the galaxy expanded, performance issues began to surface. In particular, stuttering upon loading new chunks. Whenever the player would reach the border of a chunk (@star-chunk-img), chunks would cull, and new ones would generate.

Since the instancing of hundreds or thousands of new stars made up the bulk of the operations at that time, the theory was that it was that which caused the stutters to occur. With each star possessing a MeshInstance3D @godot-meshinstance3d, and a collider (as introduced in @selectable-star-ref).

To address this, the rendering of stars was refactored to utilize Godot's MultiMeshInstance3D @godot-multimeshinstance3d. This change significantly reduced the amount of nodes instantiated in the scene, as well as the draw calls to the GPU, from one for each star to only one for each chunk (containing hundreds of stars).

This resulted in performance improvements, as can be seen in @multi-mesh-performance-table. With the average frame time remaining consistent, but the huge spikes in frame times at the 1% and 0.1% highs being greatly reduced. The average memory usage also saw a decrease from 115.9 MB to 81.7 MB.

//
// OBS. Wrong benchmarking computer.
//
#figure(
  table(
  columns: (auto, auto, auto, auto),
  inset: 7pt,
  align: left,
  table.header([],[*Average*], [*1% high*], [*0.1% high*]),
  [From], [0.39 ms], [3.88 ms], [33.43 ms],
  [To], [0.44 ms], [1.04 ms], [1.9 ms],
  ),
  caption: [Before and after frame time metrics with the multi-mesh implementation],
) <multi-mesh-performance-table>

However, this change introduced a new challenge. Since multi-mesh only instances visual meshes, and not other objects such as colliders, the stars were no longer selectable. To reintroduce star selection, two approaches were considered:

1. *Instantiate colliders at star positions*: Instantiate only a collider at each star position, but still render the meshes with the multi-mesh implementation.

2. *Ray-based selection - The "Star Finder"*: Use the known positions of stars in space, and when the player clicks, cast a ray in that direction. At regular intervals along the ray, check the surrounding area for any star positions falling within a set radius of the ray.

The second option was implemented as a system called "Star Finder", which again allows for interaction with stars, despite them only consisting of a visual mesh. Achieved via ray-casting and distance checks to the ray at regular intervals, iterating through the array of star positions of the current chunk. The Star Finder can be seen in action in @star-finder-img, with the interval and radius of proximity checks (the blue spheres) are regular in order to not miss any stars. The radius of each check also increases the further from the start position it gets, to make selection of distant stars easier.

The first option would have allowed for simpler logic in star selection, but would also include a greater load on Godot's collider calculations, as well as keep the scene hierarchy filled with hundreds/thousands of instantiated colliders. The instancing of these colliders themselves would have likely contributed to performance deterioration, since the large amount of instancing of nodes were suspected to be the cause of the stuttering from the start.

#figure(
  image("images/Galaxy/star_finder.PNG", width: 80%),
  caption: [Star Finder],
) <star-finder-img>

=== Seamless systems #text(red)[Jacob klar? Kanske inte helt 100 än hur vi gör]

In combination with great improvements in optimizing planet generation, as detailed in @planet-optimize-ref, new opportunities emerged. Previously, transitioning from the galaxy scale into individual solar systems was a static process, triggered by a button click (@galaxy-map-navigation-ref), which then loaded a separate system scene. But now, system scenes could be dynamically instantiated in real-time as the player moves towards a star. This allows the galaxy to be populated by fully realized solar systems that load seamlessly during exploration. An example of this can be seen in @seamless-systems-img.

#figure(
  image("images/Galaxy/seamless_galaxy.PNG", width: 90%),
  caption: [Seamless systems],
) <seamless-systems-img>


= Result #text(red)[ANTON KLAR]
The following chapter presents the final result of the project. First, a brief overview of the final product is provided in @result-overview. Then, in subsequent sections, a more in-depth look at the final product will be presented, in the style of a live-demonstration, focusing on the typical user experience and what can be expected of the developed application.

== Overview #text(red)[ANTON KLAR]<result-overview>
In the end, all of the must-have, should-have and the majority of the could-have features from the MoSCow table presented in @features were implemented. 

The final product includes a galaxy map where users can freely navigate between stars, where each star represents the central body of its own solar system. The galaxy map and all systems are generated deterministically, meaning that each time a user enters the same system, it will be generated in exactly the same manner as the first time. 

Entering a solar system causes its belonging celestial bodies (planets and moons) to be generated. The celestial bodies are generated procedurally using 3D-noise as well as the marching cubes algorithm, which enables complex terrain generation with overhangs. The moons have craters and a "moon-like" appearance to them and planets are colored based on their distance from the central sun; the closest planets have a "warmer" color palette while the further way planets have a "cooler" color palette. Furthermore, the planets can also have oceans and vegetation generated on their surfaces. #text(red)[oops lägg till atmosfärer också]

Finally, the end product includes a robust physics engine that is capable of updating thousands of objects simultaneously in real time.

== "Technical" ge mig ett bra namn tack
abc fysik def gravitation ghijklmnopqrstuvwxyzåäö fysik

== "Demo" byt till bra namn tack
=== Galaxy#text(red)[PAUL O ANTON ver1 klar]<demo>
When the simulation begins, the user starts in the galaxy-map, which represents deep space. Here, the user can fly around and explore different stars/systems. Clicking on a star opens an information panel that displays information of the selected star and associated system, such as the color of the star and the number of planets in its solar system.

#figure(
  image("information_panel_example.png", width: 80%)
,caption: [Information Panel In Deep Space])

Clicking on the arrow ("->") button will initiate fast-travel to the selected star and will quickly transport the user close to its location:

#figure(
  image("fastTravel.png", width: 80%),
  caption: [Fast traveling to the selected star using the information panel]
)

Selecting the 'Explore' button in the information panel teleports the user "into" the star's solar system, causing the instantiation of the system scene, which in turn causes all bodies within the system to be generated.

=== System #text(red)[PAUL O ANTON ver1 klar]
Inside the system the user is able to fly around freely. It is possible to exit the system by flying  slightly beyond the orbit of the outermost planet.

While inside you can see the planets, moons, and the central star. In the bottom right corner is an option to toggle on or off the visualization of the trajectories, which can be helpful if it's difficult to see the planets or the moons. This option can be seen in the following two figures:

#figure(
  image("system_no_trajectories_example.png", width:  80%)
  , caption: [System Without Displayed Trajectories]
)

#figure(
  image("system_yes_trajectories_example.png", width: 80%)
  , caption: [System With Displayed Trajectories]
)

The planets are generated around the sun (the explored star from the previous part) and may also be generated with moons. Planets closer to the sun will appear "warmer" and planets further away will appear "colder".

#figure(
  image("looking_at_planets_with_moons.png", width: 80%)
  , caption: [Planets With Moons]
  
)

Flying close to a planet will put the player in its orbit, making them travel with it around the system. While inside a planets orbit it is possible to turn off flying by pressing _v_. When done, the player will become susceptible to the planets gravitation and will fall on to the surface of the planet, where it will then be possible to walk around. Pressing _v_ again turns the flying back on.

#figure(
  image("standing_on_planet_example.png", width: 80%)
  , caption: [Sunrise On A Planet]
)

=== Planet
blbalbabbalbalb


= Discussion


======= #text(red)[STAFFAN STRUKTUR DISKUSSION]
#text(red)[
  1. resultatdiskussion, 
  2. process/metod (vad tycker vi om hur vi jobbade, ge råd till andra, om vi hade gjort om så hade vi gjort detta) 
  3. Generaliserbarhet och validitet (hur mycket går det att generalisera resultat, validitet; hur säkra är vi på det vi gjort tex hur äkta är simuleringen), 
  4. Etik-diskussion/reflektion (ta upp i introduktionen först vilka potentiella problem om vi gör detta, i planeringskapitel och sen i diskussionen)
  5. Framtida arbete, allt vi inte hunnit med som vi vill göra (kan bli eget kapitel om långt)
]


== Result Discussion - #text(red)[TA SENARE]

#text(orange)[Performance vs. kvalitet/hur snyggt det är? kanske hade passat här? alltså atmosfärer samt att skapa planeter med färre punkter och sen skala upp dom. Jacob skrev tidigare om frame-times och att min hålla sig inom dippar av 8ms (alltså att minska "stutters" där fps plötsligt går ner när den annars har varit stabil etc)]

After considerable effort, we managed to achieve a result, but this process forces us to contemplate the underlying reasons for our actions. What exactly drove us to take these steps in the first place? What prompted each individual choice along the way? The journey was not without its challenges, and we were constantly questioning whether our decisions were justified or logical. Despite these uncertainties, we persevered and reached an outcome that validated our efforts. Yet, this outcome now prompts a retrospective evaluation of the entire process, leading us to wonder if the initial reasons for our decisions align with the final result.

The result did result in something, but this makes us think hard. WHy did we do it, why did we do that? And yes it did lead us to a result in the end!

Vad vi lyckades med i moscow-- vilka vi avklarade gjorde


=== EXO explorer differences? ... we should probably discuss it somewhere.


== Process/Method Discussion - #text(red)[METATEXT]
This sub section provides a discussion for our process and method. Discussions surrounding the overall result, usage of multiple programming languages, the chosen workflow, how AI was used, and how some things changed from the planning stage throughout the project are included.

=== Multiple programming languages - #text(green)[JONATAN, ANTON, KLAR]
This project utilized a multi-language approach within Godot, integrating C\#, GDScript, and Rust via the GDExtension system. This strategy allowed us to select the optimal language for specific tasks. Due to being officially supported and allowing for fast development iterations, C\# and GDScript was in used for most of the game logic.

For computationally intensive components, the physics engine in particular, Rust was chosen. Its strengths in raw performance, memory safety, and concurrency enabled significant optimization. We leveraged Rust's capabilities and the `rayon` crate to parallelize demanding calculations like particle sorting for octree building and the N-body force computations within the Barnes-Hut algorithm.

While this hybrid approach provided substantial performance benefits for critical sections, it introduced complexities. Managing a multi-language build process, debugging across the GDExtension boundary, and passing data between Rust and C\#/GDScript required careful setup and proved to be cumbersome on occasion. However, the overall experience was positive, confirming that leveraging each language's strengths was advantageous for achieving the project's simulation goals despite the added overhead.

=== Arbetsstruktur. Lite rörigt, särskilt mot slutet typ... - #text(red)[JONATAN]
KANBAN - ? 

Sprintar? - nej?

Branchar, feature branches - ?

TOOLS? - ??

libgdx

Bevy \<3

PR REVIEWS <<< Skriv om detta. 
Jag tycker det ändå funkat ganska bra. Man fångar många fel, och fler får sig en idé över vad alla lägger till... etc

=== Use of generative AI - #text(red)[NÅN TA, William klar]
Artificial intelligence (AI) was utilized at various stages throughout the project. During the development phase, tools such as ChatGPT and GitHub Copilot were employed to support the coding process. Copilot was also integrated into the pull request (PR) review workflow, providing quick feedback on code submissions. While AI-generated reviews were not considered substitutes for peer-reviewed evaluations, they offered an efficient means of identifying and addressing obvious issues that might otherwise be overlooked.

Additionally, AI was employed during the report writing phase. Tools such as ChatGPT and Google Gemini were occasionally used to refine written text and enhance the overall quality of the writing.

=== Project purpose #text(red)[Jacob klar]
The projects purpose underwent a greater change after the feedback from the planning report. The original purpose is what follows:

#block(
  fill: luma(230),
  inset: 8pt,
  radius: 4pt,
)[
  "The aim of this project is to simulate solar systems through procedurally generated planets, utilizing computer-generated noise such as Perlin noise, together with the marching cubes algorithm. The composition of the solar systems can vary – from a sun with a single planet to more complex systems with multiple planets and additional celestial bodies such as moons. To mimic the natural movements of these celestial bodies, a simplified physics simulation will be implemented.

  This project also aims to explore and combine different techniques for optimization to ensure that the simulation will run in a performance-efficient manner."
]

It appeared that it was unclear to what the project set out to do, which after internal discussions the project team agreed upon. Any changes, particularly to the purpose, sought to address the following three problems:

1. A great deal had been explained specifically about solar systems in the planning report, while the team, in reality, had drifted towards wanting to create an entire galaxy of solar systems instead.
2. An entire section of the planning report was dedicated to performance and optimization, as well as a part of the purpose. This played a part in making it unclear whether this projects major focus was about optimization, or something else.
3. It was unclear how this project differs from the similar bachelor's thesis project Exo Explorer@exo_exporer:2023, from a couple of years ago.

Through internal discussions, consultation with our supervisor, and study of the previously mentioned feedback, a rewrite to facilitate a clarification of project goals and a refinement of its scope, took place. The resulting purpose can be seen in @purpose-ref.

=== MoSCoW changes #text(red)[Jacob klar]
The MoSCoW method served as a tool for structuring and prioritizing the project's tasks. By categorizing features into "Must," "Should," and "Could"-have tiers, we maintained a clear structure of what features were to be worked on, and which to be prioritized.

As the project progressed, some features shifted, some were removed, and some were added, as we gained a greater understanding of the project's scope. We treated the categories and its features as agile, rather than fixed, allowing us to adjust, as we have done.

While most features remained unchanged throughout, some of the most notable changes were:

- Camera/player controls: Moved from "Should have" to "Must have" as the focus on being able to explore the planets, systems, and galaxy, was deemed very important.
- Space background: Removed from "Must have", since it was deemed not critical for project's success. Although, a space background was eventually given by itself as the stars were distributed in the galaxy.
- Galaxy traversal: Added as a "Should have" to allow for traversal at different scales. On the solar galaxy level, the solar system level, and on the planet level.
- Other celestial bodes e.g. asteroids or nebulae, as well as planet vegetation, were added as potential features to be added. In the end planet vegetation was explored.



=== Performance #text(red)[Jacob klar]
Initially, in the planning report, the key performance metrics to evaluate the application were stated as follows:

#block(
  fill: luma(230),
  inset: 8pt,
  radius: 4pt,
)[
"*Memory consumption* - Evaluating the amount of system memory utilized during execution.

*Scene generation time* – Measuring the time it takes to generate a scene.

*Frames per second (FPS)* – Assessing the rendering performance of the simulation."
]

Shortly thereafter, a target of maintaining an average of 60 FPS was determined as a concrete benchmark to strive toward. The performance varies between computer systems with different computer hardware, so this target was to be achieved on a specific benchmarking computer with specific hardware specifications, those specifications were detailed in @benchmarking-and-performance-ref.

However, as development progressed and with greater research into real-time performance, the team improved its understanding of what constitutes a smooth and responsive performance. Rather than aiming for a high average FPS, we shifted focus to consistency in frame times instead, which offers a more accurate reflection of the user experience. In particular, frame time spikes can result in noticeable stutter during runtime, but still, the average FPS will remain and appear stable. This updated benchmarking methodology, along with a more in depth explanation, can be found in @benchmarking-and-performance-ref.

Regarding the other original metrics:

- Scene generation time: Was initially marked as a performance metric, aiming to measure how long it takes to initialize and load new scenes. However, as development progressed this became less critical since the majority of elements are streamed at runtime, rather than through traditional loading screens in between. Content such as stars, systems, and planets are generated dynamically as the player explores. As long as the initial startup time remains reasonable, the performance is better reflected in runtime frame time stability, rather than by any isolated loading durations. However, some operations were measured in time it takes to complete, to easily compare performance of different implementations and/or optimizations during development. E.g. planet generation.

- Memory consumption: While initially a concern, proved not to be a limiting factor in practice. This is likely due to the procedural nature of the project, where the majority of elements are generated procedurally at runtime, rather than pre-loaded and/or stored in memory. As a result, memory usage remained relatively low throughout development. Although it continued to be monitored, but in the end no memory-related issues occurred, making it a non-critical metric for this project.

=== Benchmarking (idk, perhaps. probably place under method.) - #text(red)[TA BORT?]
Benchmarksystem?
To document improvements and declines in performance, a streamlined benchmarking system was implemented to provide an additional point of argument....

https://www.tomshardware.com/news/what-makes-a-good-game-benchmark
Discusses benchmarking practices.
Repetability is important. Mentions that AC Odyssey was notoriously inconsistent to benchmark due to random weather effects such as rain and cloud coverage having a great impact om the benchmark results. Goes well with the deterministic nature of this project.
Having the benchmark represent actual situations that the user could encounter, -- mentions is a good thing as well. An early version of the galaxy map benchmark ran at a fact pace, moving the player at a speed that won't be encountered in a typical user scenario. The speed was decreased. // kanske

*General benchmarking*
benchmarking system. fps, frame times, memory usage. runs specific scenes and tracks these.
e.g. a scene of the galaxy map (@galaxy-map-ref), testing the implications of generating new chunks of stars (@star-chunk-img) continuously, for a duration of time.

*Other, more specific, benchmarking that differs between separate features.*

...measure of milliseconds for marching cubes CPU vs GPU computations.

...physics benchmarks



== Generalizability and Validity - #text(red)[Jonatan KLAR ]
This section considers the broader applicability of the project's components and the soundness of its simulation results.

=== Generalizability
Many techniques employed in this project are highly generalizable. The implemented N-body algorithms are standard methods applicable to other systems governed by inverse-square laws (like gravity or electrostatics). The Morton-based octree construction represents an efficient approach for spatial partitioning relevant to various particle simulations. Furthermore, optimization strategies like octree-driven Level of Detail (LOD), chunking, multi-mesh instancing, and CPU parallelism (for example using `rayon`) are standard practices widely used across real-time 3D graphics and game development for managing large-scale environments and computations. The procedural generation techniques (noise, Marching Cubes) are also broadly applicable. Finally, the integration with Godot via GDExtension also demonstrates a general pattern for offloading heavy computations from the game engine to a high-performance Rust backend.

=== Validity
Evaluating the validity of this project requires considering the goal of achieving the most scientifically accurate physics model possible within the constraints of a smooth, real-time simulation.

1. *Physics Simulation*: The simulation is grounded in established physical principles, using Newton's Law of Universal Gravitation and standard N-body algorithms like Direct Summation and Barnes-Hut. The choice of the Barnes-Hut method provides a computationally tractable way $O(N "log"N)$ to handle large numbers of bodies, which is essential for simulating galactic scales in real-time. However, achieving the necessary performance demanded certain trade-offs regarding accuracy:

  - _Integration Method_: We implemented the symplectic Euler method for time integration. While selected for its improved stability over explicit Euler (crucial for preventing orbits from rapidly degrading in real-time), it is a first-order integrator. This means it inevitably introduces numerical errors and does not conserve energy perfectly over long simulations, unlike higher-order symplectic methods (e.g. Leapfrog or higher-order Runge-Kutta @numerical_recipes) often used in non-real-time scientific studies. This choice represents a direct trade-off between achievable accuracy and the computational budget per frame needed for smoothness.
    
  - _Omitted Physics_: To maintain real-time feasibility, complex physical interactions beyond basic Newtonian gravity were omitted. This includes relativistic effects, physical collisions between bodies and non-gravitational forces.
  
  - _Outcome_: The resulting simulation produces visually plausible orbital mechanics suitable for an interactive exploration context, but its predictive accuracy for scientific purposes is limited by these necessary simplifications.

2. *Procedural Generation:* The validity here pertains to internal consistency and alignment with aesthetic goals. The use of deterministic, seeded generation ensures that the galaxy and its systems are consistently reproducible. While the generation employs physically inspired concepts (noise, fBm), the resulting planetary features and system architectures prioritize visual diversity and exploration potential over strict adherence to models of astrophysical formation.

In essence, the project's validity lies in its successful implementation of recognized N-body algorithms and optimization techniques to create a large-scale, real-time simulation that aims for physical realism while accepting necessary compromises to ensure interactivity and performance targets were met.


== Societal and Ethical aspects #text(red)[ERIK KLAR]
During the planning phase of this project, two primary ethical and societal concerns regarding the use of procedural content generation (PCG) in game development were identified. These concerns were: the potential displacement of game designers by PCG algorithms, and the risk of generating content that lacks sufficient variety, leading to a repetitive player experience.

The first point is related to the concern about game designers losing their jobs to procedural generation algorithms. However, the development process in this project revealed that significant manual effort remained necessary. Generating aesthetically pleasing content and scaling and positioning the celestial bodies in a plausible manner all required substantial manual tweaking of parameters and settings. Moreover, PCG algorithms have the potential to generate content that may be faulty, unrealistic or unnatural. Solutions to these issues include setting specific constraints before executing the algorithms, or involving a human designer at the end of the content generation #cite(<computers13110304>). Although it is still a possibility that future PCG algorithms can automate these processes, current implementations seem to depend on a collaborative relationship between human designers and algorithmic systems. 

The second concern focused on the potential for PCG to produce content that is overly repetitive, thereby decreasing the quality of the user experience. While the primary objective of this project was not to create an engaging game play experience, certain measures were nonetheless taken to mitigate repetitiveness. For instance, planetary coloration was randomized based on each planet’s distance from the sun, with additional randomization applied to simulate variations in atmospheric thickness. These methods introduced greater diversity in the generated content, demonstrating that careful parameterization and randomness can effectively counteract some of the inherent risks associated with procedural generation.


== Future work - #text(red)[William klar]
There are several directions in which this project could be expanded. A number of planned features were not implemented due to time constraints, and these could serve as valuable additions in future iterations.

In particular, the planet generation system offers significant room for enhancement. At present, the generated planets include basic features such as vegetation (e.g., trees and grass) and bodies of water (e.g., oceans), but they remain relatively un-engaging. Future improvements could include the addition of fauna, subterranean structures such as cave systems, and other biome-specific features to increase diversity and immersion within the planet.

Another potential extension involves incorporating a wider range of celestial bodies, such as gas giants, meteoroids, and comets. This would significantly enhance the diversity and complexity of the planetary generation system. A realistic galaxy comprises various types of astronomical objects, not solely solid-surface planets. Currently, the project does not convey this diversity, and expanding the range of celestial bodies would contribute to a more authentic and immersive galactic environment.

Enhancing the physics system is another area which could be expanded upon. Currently, the player character is not fully integrated into the simulation of celestial bodies; gravitational effects are applied only when the player is in close proximity to a planet, rather than being simulated by the implemented physics engine (see #ref(<physics-engine-ref>) and #ref(<player-controls-ref>)). Integrating the player into the same physics framework as the celestial bodies would increase realism and coherence within the simulation.

Overall, the project presents many opportunities for refinement and expansion, particularly in the areas of planetary diversity and physical simulation. Enhancing these aspects would contribute to a more engaging and immersive user experience.



= Conclusion - #text(red)[Jonatan KLAR ISH]
This project set out to develop and simulate a physics-based, procedurally generated, and explorable galaxy within the Godot engine, addressing the inherent challenges of computational scale, performance, and plausibility in creating such vast virtual environments. The core objective was to produce a deterministic and computationally efficient model capable of real-time interaction.

Development resulted in a comprehensive system. Planets were successfully generated using a combination of noise functions (Perlin, fBm) and the Marching Cubes algorithm, resulting in complex and varied terrains. These planets were further enhanced with procedurally placed features, including oceans with shader-based wave effects, vegetation distributed via Poisson-disc sampling, and physically-inspired atmospheres simulating Rayleigh scattering. Crucially, significant performance optimizations were implemented and validated; multi-threading effectively offloaded intensive planet generation tasks, while octree-based chunking with Level of Detail (LOD) dynamically managed mesh complexity, and MultiMesh instancing drastically reduced draw calls for star rendering. A robust N-body physics engine, leveraging a parallelized Barnes-Hut algorithm implemented in Rust, was integrated to simulate celestial mechanics efficiently for numerous bodies. The use of seeded randomization throughout ensured deterministic generation, allowing for reproducible galaxies and solar systems. Exploration capabilities were successfully implemented, allowing navigation across multiple scales via a galaxy map and seamless transitions into solar systems down to planetary surface interaction with a physics-aware controller.

The project successfully met its primary goals, fulfilling the essential "Must Have" and "Should Have" requirements defined during planning. The focus on achieving consistent frame times, rather than solely maximizing average FPS, proved effective in delivering a smoother user experience during exploration and simulation.

Ultimately, this work contributes a practical implementation and analysis of techniques essential for simulating large-scale procedural galaxies. It demonstrates the successful integration of advanced procedural generation, N-body physics simulation, and targeted optimization strategies within the Godot engine, offering a viable and efficient model for developers aiming to create expansive, dynamic, and interactive celestial environments. While acknowledging necessary simplifications for real-time performance, the project establishes a solid foundation upon which future enhancements, such as greater celestial diversity or more complex physical interactions, can be built.




#pagebreak()
#bibliography("src.bib")












#pagebreak()

#v(200pt)
= Kladd - #text(red)[Flytta in till relevant om vi vill] <kladd>

== Fundamentals of 3D Graphics for Procedural Planet Generation

This section outlines how we utilize the GPU and the *rendering pipeline* for our procedural planet generation project.

=== GPU Architecture and the Rendering Pipeline

Modern GPUs are *massively parallel processors*. Unlike CPUs, which typically have a few very fast and versatile cores, GPUs contain *many* cores optimized for performing the same operation on many data points simultaneously. This makes them ideally suited for the numerous independent calculations involved in 3D rendering. We will make extensive use of the GPU's *rendering pipeline*, specifically leveraging *vertex shaders*, *rasterization*, *fragment shaders*, and output merging to transform our 3D planet data into a 2D image.

We will optimize our rendering process to leverage the GPU's parallel capabilities, achieving *real-time* performance.

=== Meshes: Representing 3D Objects

Our planets are represented as *meshes*, collections of interconnected triangles. Each *vertex* in a mesh stores its position, a *normal* vector, and *UV coordinates*.

=== Key Concepts and Considerations

- *Real-time Rendering:* We require real-time rendering for an interactive experience, maintaining a smooth frame rate.

- *Performance Optimization:* We'll employ various optimization techniques:
    - *LOD (Level of Detail):* Using different mesh complexities based on distance.
    - *Occlusion Culling:* Avoiding rendering hidden objects.
    - *Compute Shaders:* Potentially using *compute shaders* to accelerate terrain generation or physics calculations.

- *Procedural Generation:* We'll use *noise functions* and the *marching cubes* algorithm to generate varied and realistic planet surfaces.

=== Relevance to the Project

- *Efficient Terrain Generation:* Optimizing the mesh generation process is essential for performance.
- *Realistic Visuals:* *Fragment shaders* are crucial for lighting, texturing, and atmospheric effects.
- *Interactive Exploration:* Maintaining a high frame rate requires careful optimization of the entire *rendering pipeline*.


== Anton saker

Water:

Scrolling noise textures and bump mapping..

Scrolling noise for vertex displacement..

Scrolling 3d noise..

Problems: pinching at poles - future fix with e.g. triplanar uv mapping, cube sphere, but maybe OK for now? Also occasianly flickering.. clash with atmosphere?

Depth coloring using screen texture for transparency (multiply the normal color with water color) and depth texture for interpolating color based on depth. Foam color by mixing in white at edges (where depth value is low)..

Using opaque rendering mode so it is included in the depth texture..

Possible improvements for better visuals: beers law, fresnel



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

+ How should the planet generation be accomplished?
+ How could the planets be further distinguished from each other, apart from using noise?
+ In what way should the solar system be generated procedurally?
+ 

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


== Noise
As mentioned in the previous section, procedural generation algorithms work by taking data input and producing generated content out of that, which in #text(yellow)[our] case would be terrain. 
However that arises is how #text(red)[we] get these data points. Instead of manually getting them #text(red)[we] can instead use something called noise functions to get these data points. \
In @Shaker2016 noise functions are described as “_the random number generator of computer graphics_”. There exists different kinds of noise functions which generate random noise in different ways and for procedural terrain generation the relevant noise functions as mentioned by @noisefunctions1:2016 includes: diamond-square algorithm, value noise, perlin noise, simplex noise, and Worley Noise. What will probably be most present in this project however is perlin noise.

== metod

#text(maroon, size: 12pt)[
  ???
  
  Our project will rely heavily on real-time 3D graphics techniques. #text(red)[We] will leverage the GPU's massively parallel architecture and its rendering pipeline, including vertex and fragment shaders, to efficiently render procedurally generated planets. Key optimization strategies will include level of detail (LOD), occlusion culling, and potentially compute shaders. #text(red)[We] will represent planets as triangle meshes, utilizing procedural generation techniques based on noise functions and the marching cubes algorithm. A glossary of key 3D graphics terms is provided in Appendix X... 
]

#text(red)[ Kanske inte behövs
=== Light
Simulating illumination in the systems is important for player experience, not only because it's desirable when things look "better". But also because illumination is often an important differentiator between planets. (Different colored atmosphere, no atmosphere, gas giants etc.)

Simulating light correctly can be difficult but it is helpful that, in general, detail is only needed once the player is close to a planet. Which would mean that the player is far away from other planets, opening the possibility for big performance gains due to only one having to render one planet in detail at a time.]












#show: appendix

#pagebreak()
= \ Pseudocode for Barnes-Hut algorithm <bh_pseudo>
#algorithm({
  import algorithmic: *
  State[*const* $theta$]
  State[*const* $epsilon$]
  State[]

  Function("Barnes-Hut-Gravity", args: ("G", "particles"), {
    Cmt[Get the number of particles]
    Assign[$n$][#FnI[length][particles]]

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
  import algorithmic: *

  Function("recurse", args: ("G", "node", "all_particles", $i$, $"pos"_i$), {
    Cmt[Get node properties (mass, center of mass, size)]
    Assign[$m_n$][node.*mass*]
    Assign[$"CoM"_n$][node.*CoM*]
    Assign[$s_n$][#FnI[width][node]]

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
  import algorithmic: *

  Function("calcAcc", args: ($G$, $m$, $r$), {
    Assign[$d_"soft"$][$sqrt(r^2 + epsilon^2)$]
    Assign[$a$][$G * m / d_"soft"^3 * r$]
    Return[a]
  })
})
