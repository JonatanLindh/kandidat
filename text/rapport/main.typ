#import "@preview/numbly:0.1.0": numbly
#import "@preview/algorithmic:0.1.0"
#import algorithmic: algorithm
#import "@preview/timeliney:0.2.0"
#import "@preview/headcount:0.1.0": *

#show figure.where(
  kind: table
): set figure.caption(position: top)

#let appendix_numbering

#let appendix(body) = {
  set page(numbering: "I")
  show heading: set align(center)

  set figure(numbering: dependent-numbering("A-1"))
  show heading: reset-counter(counter(figure.where(kind: table)))
  
  set heading(
    numbering: numbly("Appendix {1:A}"),
    supplement: [],
    hanging-indent: 0pt,
  )
  counter(heading).update(0)
  counter(page).update(1)
  
  [#body <no-wc>]
}

#let in-outline = state("in-outline", false)
#show outline: it => {
  in-outline.update(true)
  it
  in-outline.update(false)
}

#let flex-caption(long: none, short: none) = context if in-outline.get() { short } else { long }

#set heading(numbering: "1.")


#page()[
  #grid(
    rows: (1fr, 5fr, 1.5fr, 1fr),
    [
      #image("chalmers_gu.png")
      
      #line(length: 100%)
    ],
    [      
      #image("maybe_nice_2.png", width: auto)

      #text(weight: "bold", size: 30pt)[
        Galaxy Engine
      ]
      
      #text(size: 17pt)[
        Simulating a physics-based procedurally generated galaxy
      ]

      //#set text(size: 17pt)
      //Creating Solar Systems of 
      //Procedurally Generated Planets
      
      #text(size: 13pt)[
        Bachelor’s thesis in Computer science and Engineering
      ]
    ],
    [
      #set text(size: 14pt, weight: "bold")
      Jacob Andersson \
      Erik Berglind \
      Anton Frejd \
      William Karlsson \
      Jonatan Lindh \
      Paul Soukup
    ],
    [
      #line(length: 100%)
      #set text(size: 13pt)
      #set align(bottom)
      Department of Computer Science and Engineering \
      #smallcaps[Chalmers University of Technology] \
      #smallcaps[University of Gothenburg] \
      Gothenburg, Sweden 2025
    ]
  )
]

#page[]

#page[
  #set align(center + horizon)
  #set text(size: 16pt)
  
  #grid(
    rows: (1fr, 10fr, 1fr),
    smallcaps[
      #set align(top)
      Bachelor's Thesis 2025
    ],
    [
      #text(weight: "bold", size: 30pt)[
        Galaxy Engine
      ]
      
      #text(size: 17pt)[
        Simulating a physics-based procedurally generated galaxy
      ]
      
      #v(1em)

      Jacob Andersson \
      Erik Berglind \
      Anton Frejd \
      William Karlsson \
      Jonatan Lindh \
      Paul Soukup

      #v(2em)
      
      #image("chalmers_gu_vertical.png", width: 35%)
    ],
    [
      #set align(bottom)
      
      Department of Computer Science and Engineering \
      #smallcaps[Chalmers University of Technology] \
      #smallcaps[University of Gothenburg] \
      Gothenburg, Sweden 2025
    ]
  )
]


#set page(numbering: "i")

#page[
  #align(left + horizon)[
    #text(18pt)[Galaxy Engine]
    
    #text(15pt)[_Simulating a physics-based procedurally generated galaxy_]

    #set text(size: 12pt)
    
    Jacob~Andersson, Erik~Berglind, Anton~Frejd,  
    William~Karlsson, Jonatan~Lindh, Paul~Soukup  
    
    Department of Computer Science and Engineering  
    Chalmers University of Technology and University of Gothenburg
    
    #v(15pt)
    
    $copyright$ Jacob~Andersson, Erik~Berglind, Anton~Frejd, William~Karlsson, Jonatan~Lindh, Paul~Soukup 2025.
    
    Supervisor: Staffan Björk, Department of Computer Science and Engineering\
    Graded by teacher: Aris Alissandrakis, Department of Computer Science and Engineering \
    Examiners: Arne Linde and Patrik Jansson, Department of Computer Science and Engineering
    
    #v(15pt)
    
    Bachelor's thesis 2025\
    Department of Computer Science and Engineering\
    Chalmers University of Technology and University of Gothenburg\
    SE-412 96 Gothenburg\
    Telephone +46 31 772 1000
  
    #align(bottom)[
      Cover: A view of a planet (left) and its moon (right), with the solar system's star in the middle.\
      Link to the GitHub repository: #link("https://github.com/JonatanLindh/kandidat")
    
      #v(1em)
      
      Typeset in Typst\
      Gothenburg, Sweden 2025
    ]
  ]
]




#pagebreak()
#align(left)[
  #text(14pt)[Galaxy Engine]  
  
  #text(12pt)[
    _Simulating a physics-based procedurally generated galaxy_
  ]  
  
  Jacob~Andersson, Erik~Berglind, Anton~Frejd,  
  William~Karlsson, Jonatan~Lindh, Paul~Soukup  
  
  Department of Computer Science and Engineering  
  Chalmers University of Technology and University of Gothenburg
]

#heading(numbering: none, outlined: false, bookmarked: true)[
  Abstract
]

Procedural Content Generation (PCG) enables the algorithmic creation of vast virtual worlds, particularly relevant for space exploration simulations, yet poses significant challenges regarding computational scale, performance, and plausibility. This project addresses these challenges by developing and simulating a physics-based, procedurally generated, and explorable galaxy within the Godot game engine. The core objective was to create a deterministic and computationally efficient model. Planets were generated using noise functions (Perlin, fBm) combined with the Marching Cubes algorithm to create complex terrains, enhanced with procedurally placed oceans, vegetation, and physically-based atmospheres simulating Rayleigh scattering. Optimization was crucial, employing techniques such as multi-threading for planet generation, chunking, and Level of Detail (LOD) managed via octrees. A custom N-body physics engine, featuring a parallelized Barnes-Hut algorithm ($O(N "log"N)$) built upon a Morton-code-based linear octree, simulates celestial mechanics. Solar systems and the galaxy structure utilize seeded randomization for determinism and reproducibility. Exploration spans multiple scales, from a galaxy level down to planetary surface navigation with a physics-aware controller. The project successfully yielded a real-time simulation, demonstrating efficient generation and rendering of diverse celestial bodies and stable orbital physics, prioritizing consistent frame times. This work contributes a practical implementation and analysis of techniques for large-scale procedural galaxy simulation in Godot.

#align(bottom)[Keywords: Procedural content generation, planet, solar system, galaxy, physics simulation, Barnes-Hut simulation, noise, Godot]

#pagebreak()

#text(lang: "SE")[
  #heading(numbering: none, outlined: false, bookmarked: true)[
    Sammandrag
  ]

  Processuell Innehållsgenerering (PCG) möjliggör algoritmiskt skapande av enorma virtuella världar, vilket är särskilt relevant för rymdutforskningssimulationer. Metoden medför dock betydande utmaningar gällande beräkningsmässig skala, prestanda och trovärdighet. Detta projekt adresserar dessa utmaningar genom att utveckla och simulera en fysikbaserad, processuellt genererad och utforskningsbar galax i spelmotorn Godot. Huvudmålet var att skapa en deterministisk och beräkningseffektiv modell. Planeter genererades med hjälp av brusfunktioner (Perlin och fBm) i kombination med Marching Cubes-algoritmen för att skapa komplex terräng. Denna terräng förbättrades sedan med processuellt placerade hav, vegetation och fysikbaserade atmosfärer som simulerar Rayleigh-spridning. Eftersom optimering var avgörande användes tekniker som flertrådning för planetgenerering, "chunking" och detaljnivåer (LOD). En specialbyggd N-kropps-fysikmotor simulerar himlakroppars mekanik. Motorn använder en parallelliserad Barnes-Hut algoritm ($O(N "log"N)$), byggd på ett linjärt octree baserat på Morton-kod. Solsystem och galaxens struktur baseras på "seedad" slumpmässighet för att säkerställa determinism och reproducerbarhet. Utforskning är möjlig på flera skalor, från galaxnivå ner till navigering på planetens yta med fysikbaserad styrning. Projektet resulterade i en framgångsrik realtidssimulering som demonstrerar effektiv generering och rendering av varierande himlakroppar samt stabil omloppsfysik. Fokus låg på att uppnå konsekventa bildrutetider. Detta arbete utgör ett praktiskt bidrag i form av en implementation och analys av tekniker för storskalig processuell galaxsimulering i Godot.

  #align(bottom)[
    Nyckelord: Processuell innehållsgenerering, planet, solsystem, galax, fysiksimulering, Barnes-Hut-simulering, brus, Godot
  ]
]


#pagebreak()
#heading(numbering: none, outlined: false, bookmarked: true)[
  Acknowledgements
]

We would like to thank our supervisor Staffan Björk for his invaluable support throughout the project. From telling funny anecdotes to being a mentor, he has always been there to provide many interesting ideas and answers to even our most difficult questions. 

Jacob~Andersson, Erik~Berglind, Anton~Frejd,  
William~Karlsson, Jonatan~Lindh, Paul~Soukup

May 2025

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
- *Player*: User controlled element in the simulation.
- *Graphics Processing Unit (GPU)*: Computer hardware component that can effectively perform many, parallel, mathematical computations and is often used for rendering computer graphics @GPUBrittanica.
- *Central Processing Unit (CPU)*: Central hardware component of a computer composed of the control unit, main memory, and arithmetic logic unit  @CPUBrittanica.

- *Mesh:* A collection of vertices, edges, and faces that define the shape of a 3D object @mesh.
  - *Vertex:* A point in 3D space, representing a corner of a triangle. Stores attributes such as position (x, y, z), normal, UV, and optionally color.
    - *Normal:* A vector that is perpendicular to a surface. Used in lighting calculations.
    - *UV coordinates:* a 2D coordinate system used to map a texture image to a 3d object surface.
  - *Edge:* A line segment connecting two vertices.
  - *Face (Triangle):* A flat surface bounded by edges (typically a triangle in real-time graphics). Triangles are used because they are always planar (all vertices lie in the same flat plane).
  
- *Rendering Pipeline:* The sequence of stages a GPU uses to render a 3D scene @rendering-pipeline.

- *Shader:* A small program that runs on the GPU, primarily used for controlling how objects are rendered @gpu-shader.
    - *Vertex Shader:* Processes each vertex and transforms vertex attributes, such as position @vertex-shader.
    - *Fragment Shader (Pixel Shader):* Determines the final color of each fragment (roughly corresponding to a screen pixel), often by using lighting and textures @fragment-shader.
    - *Compute Shader:* Used for general-purpose computation on the GPU (not directly part of the rendering pipeline) @compute-shader.
    
- *Procedural Content Generation (PCG):* The creation of data (models, textures, etc.) algorithmically, rather than manually. This often uses noise functions and other mathematical techniques to create varied and complex results @proceduralgame:2016.
  - *Seed:* A value used to initialize a pseudo-random number generator. Using the same seed ensures the same sequence of "random" numbers @godot-random-number-generator.
  - *Noise Function:* Algorithms for generating pseudo-random values with a smooth, continuous appearance, used for procedural generation @proceduralgame:2016 @perlinnoise:1985. 
  - *Marching Cubes:* Algorithm to create triangle meshes from 3D scalar fields (e.g., noise) @marchingcubes:1998.

- *Level of Detail (LOD):* Rendering objects with varying complexity based on distance @RTR4.

- *Performance metrics:*
    - *Frames per second (FPS):* The amount of images (frames) that a computer renders every second @nvidia_fps. Higher FPS generally means smoother motion. For example, 60 FPS means the screen is updated 60 times per second. Inversely related to frame time.
    - *Frame time:* The amount of time (in milliseconds) it takes to render a single frame @techreport_inside_the_second. Inversely related to FPS.
    
- *Godot terminology:* @godot-nodes-and-scenes
    - *Node:* A fundamental building block in Godot for creating game elements. Nodes can represent various components such as images, 3D models, cameras, colliders, sounds, and more.
    - *Tree:* An arrangement of nodes organized hierarchically, where each node can have child nodes.
    - *Scene:* A collection of nodes arranged as a tree, which can be saved as a new reusable self-contained node. Scenes can be instantiated multiple times throughout an application. For example, a Player Character Scene might include nodes for the character’s image, collider, and camera, all grouped together for easy reuse.

#set page(numbering: "1")
#counter(page).update(1)

= Introduction
Procedural content generation (PCG) offers a way to algorithmically create vast and diverse game worlds without the immense manual effort required to design every detail. Applications of PCG range from the random placement of enemies in confined dungeon spaces to the generation of entire universes comprising millions of celestial bodies. Using PCG also has the potential to increase re-playability @PCGNeuroevolution.

Using PCG algorithms is particularly relevant in the context of space exploration games, where the scale of the universe is inherently beyond manual creation. However, creating compelling, varied, and believable planetary systems and galaxies is a challenging problem within this domain. Key issues include the computational efficiency required to generate hundreds to millions of celestial objects, as well as the need to balance performance constraints with the goal of providing a plausible and playable experience.

This thesis addresses these challenges by developing a system for procedurally generating a galaxy composed of multiple solar systems, with an emphasis on computational efficiency and physical accuracy. The implementation is built in the Godot game engine, and requires utilization of relevant techniques, such as GPU computation, diverse terrain generation algorithms, and physical models. In addition, the project documents key development decisions to offer insight into addressing such challenges.

A related project, Exo Explorer @exo_exporer:2023, a bachelor's thesis from Chalmers University of Technology, explored similar themes. While Exo Explorer focused on a single solar system and emphasized gameplay and planetary ecosystems, this thesis places a greater focus on simulating a physically accurate model of a procedurally generated, explorable galaxy, rather than emphasizing planet-level details.

== Purpose <purpose-ref>
The aim of this project is to create a physics-based simulation of a procedurally generated, explorable galaxy. 

Each solar system within the galaxy will contain procedurally generated planets orbiting a central star, governed by a simplified Newtonian physics simulation. System complexity will range from a single planet to multiple planets and moons.

While procedurally generated, the galaxy will remain persistent and revisitable by ensuring deterministic generation. Different seeds allow for unique galaxy creation, while ensuring specific regions generate identically upon revisit.

== Limitations <I-limitations>
Key project limitations were established. The physics simulation was planned to be simplified, prioritizing plausible orbital mechanics for solar systems over strict adherence to real-world gravitational laws or all physical intricacies. 

Furthermore, this project was going to be developed as a technical demonstration, not an actual video game. The primary focus was on terrain generation, physics simulation, and performance optimization. Consequently, gameplay-oriented features such as extensive UI for navigation or detailed planetary properties were not prioritized, as the goal was a procedurally generated galaxy model, not an engaging gameplay or narrative experience. That said, for clarity and convenience, we will still use the terms player and gameplay, even though the project is more accurately described as a simulation.

== Contribution
This thesis hopes to contribute a computationally efficient, conceptually interesting, and accurate physics model for simulating celestial movements, as well as a scalable procedural generation system for populating game environments, implemented within the Godot engine.

= Background
This section presents the foundational theoretical concepts that were needed before beginning the project, followed by a few select previous works that utilize some of these concepts.

== Procedural Content Generation
Procedural Content Generation (PCG) is defined as “the algorithmic creation of game content with limited or indirect user input” @proceduralgame:2016. While PCG is widely used in video games—typically involving the automatic generation of content such as unique levels for each gameplay session or the stochastic placement of environmental elements such as vegetation—it also has applications beyond gaming, such as in architectural design, and data generation.

== Noise <background-noise-ref>
In PCG, noise refers to pseudo-random, often spatially coherent, data used to introduce controlled variation and natural-looking patterns. It is typically generated by functions that produce a value for any given input point (e.g., coordinates) @proceduralgame:2016.

Noise is very commonly used in PCG, such as for generating mountains, textures and vegetation @proceduralgame:2016. It is generated through the use of a pseudo-random function and is often stored and visualized as a texture.

Perlin noise @perlinnoise:1985 is a famous gradient noise function developed by Ken Perlin in 1985 that has been used extensively to produce procedural content in games and films.

== Terrain Generation
This subsection provides an overview of height-maps and the Marching Cubes algorithm, the two techniques used to procedurally generate terrain in the project.

=== Height-maps <height-maps>
Height-maps serve as data structures encoding elevation information, often applied to vertices of a mesh geometry @heightmaps:2019. Typically, height-maps are implemented as grayscale image files, where the intensity of each pixel corresponds to the height at a specific location on the mesh. This representation is computationally efficient and widely used in terrain generation and visualization. However, height-maps have inherent limitations. Notably, they are capable of representing only a single elevation value per (x, y) coordinate. As a result, they are unsuitable for modeling complex terrain features that involve vertical structures or vertical overhangs, such as caves and arches.

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
        caption: [3d render using the height-map texture]
      )
    ]
  )
)

=== Marching Cubes <marching-cubes>
The Marching Cubes algorithm @marchingcubes:1998 is a method for extracting a polygonal mesh representation of an isosurface from a three-dimensional scalar field. An isosurface is a surface that represents points of a constant value within a volume. A scalar field, on the other hand, is a "_scalar-valued function of the points of a domain in some space, such as the temperature or the density field inside a body_" @scalarfield. The algorithm takes a grid of scalar values as input and generates a mesh approximating the isosurface defined by a given threshold, enabling the visualization of complex structures within the data.

The procedure involves traversing the scalar field and evaluating groups of eight adjacent grid points, which form a logical cube. For each cube, the algorithm determines the polygon(s) that approximate the isosurface intersecting that region. This is achieved by classifying each vertex of the cube as either inside or outside the isosurface, based on whether its scalar value (the value at that vertex in the scalar field) is above or below the predefined iso-level (the chosen threshold value that defines the isosurface). 
In other words, the iso-level specifies the value that the isosurface represents, while the scalar value is the actual value at a specific vertex. The algorithm then references a precomputed lookup table to identify the appropriate triangulation for the cube's configuration.

Given that each of the eight cube vertices can exist in one of two states (on or off), there are $2^8 = 256$ possible configurations. However, due to symmetry and rotational equivalence, these reduce to 15 unique cases (see #ref(<mc15>)), with all others derivable through reflection or rotation. The process is repeated throughout the entire scalar field, and the resulting polygons are combined to form the final mesh.

#figure(
  image("images/MarchingCube/MarchingCubesEdit.svg", width: 65%),
  caption: flex-caption(
    short: [Marching Cubes' 15 unique polygon combinations.
    ],
    long: [Marching Cubes' 15 unique polygon combinations. Ryoshoru, #link("https://creativecommons.org/licenses/by-sa/4.0")[CC BY-SA 4.0], via Wikimedia Commons
    ]
  )
) <mc15>

== Chunks <B-chunks>
Chunks @Chunk1 refer to (in this context) fixed-size segments of data that are loaded, processed, and rendered independently. This approach is particularly beneficial in large or procedurally generated environments, as it allows the game engine to manage memory and computing resources efficiently by loading only chunks near the player.

=== Octrees  <B-octree>
An octree @octree1 is a hierarchical data structure that recursively subdivides three-dimensional space into eight octants using a tree structure (see #ref(<octreeimg>)). This structure is particularly effective for managing sparse or large-scale environments, as it allows for efficient spatial queries, collision detection, and level-of-detail (LOD) rendering.​

Chunks are typically uniform, fixed-size sections of the game world that load and unload as needed, but octrees provide a more dynamic alternative.

#figure(
  image("images/Octree/Octree2.svg.png", width: 65%),
  caption: flex-caption(
    short: [Visualization of an Octree structure in both a cube and tree format.],
    long: [
      Visualization of an Octree structure in both a cube and tree format. WhiteTimberwolf, #link("https://creativecommons.org/licenses/by-sa/3.0/deed.en")[CC BY-SA 3.0], via Wikimedia Commons
    ]
  )
)<octreeimg>

== GPU Computation
*GPU* computing is the process of utilizing the highly parallel nature of the GPU for running code. Since the GPU has significantly more processing units than the *CPU*, it can be utilized to write highly parallelized pieces of code to solve certain programming problems @princeTonGPU. Programs that run on the GPU are often referred to as shaders @UnityComputeShaders.

== Previous works
Several existing games and research projects provide a foundation for this work, demonstrating both the potential and the challenges of procedural planet and solar system generation and simulation:

=== Minecraft
While not focused on planetary systems, Minecraft @minecraft:2009 demonstrates the power of procedural generation in creating vast and varied landscapes using noise functions (explained in @background-noise-ref) and height-maps (explained in @height-maps). In Minecraft, noise is used to procedurally create terrain features such as mountains, valleys, and cave systems. This approach results in endless, unique environments for the player to explore.

#figure(
  image("images/PreviousWorks/minecraft_landscape.png", width: 65%),
  caption: flex-caption(
    short: [Minecraft Windswept Hills biome
    ],
    long: [Minecraft Windswept Hills biome, CC BY-NC-SA 3.0, Mojang Studios \u{00A9}
    ]
  )
)

=== Outer Wilds
Outer Wilds @OuterWilds0 is a space exploration and adventure game that bears a small resemblance to this project. Unlike the other works mentioned, which are related through their use of procedural generation, Outer Wilds features entirely hand-crafted content. Its relevance to this project instead lies in its approach to physics. In Outer Wilds, all physics interactions are computed in real-time, with no pre-defined behaviors. For instance, planetary motion is governed by a modified version of Newton's law of gravitation, and all velocities are dynamically calculated during gameplay @outerwilds1.

#figure(
  image("images/PreviousWorks/outerwildsmapimg.png", width: 75%),
  caption: flex-caption(
    short: [Map of the Outer Wilds solar system.
    ], 
    long: [Map of the Outer Wilds solar system. Outer Wilds, Mobius Digital \u{00A9}, 2019. Screenshot taken in-game. Used under Mobius Digital's Fan Content Policy & Guidelines
    ]
  )
)

=== Exo Explorer
Exo Explorer @exo_exporer:2023 is an earlier bachelor's project, also from Chalmers, which directly addressed the challenge of procedurally generating solar systems using the Unity engine @unity. The project utilized Perlin noise @perlinnoise:1985 and the Marching Cubes algorithm @marchingcubes:1998 to create planet terrain featuring forests, lakes, and creatures with basic simulated needs (hunger, thirst, reproduction).

Exo Explorer provided valuable inspiration for this project by demonstrating techniques in procedural generation and optimization, among others. However, this project seeks to expand on that foundation. It aims to delve deeper into aspects such as the complexity of simulated physics, performance, and exploration. A key distinction is this project's greater focus on simulating multiple, simultaneously explorable solar systems.

= Planning <Planning>
This section describes the methodology and planning behind the project. It presents the chosen workflow, the selected tools and technologies, and the intended features.

== Workflow <Workflow>
Development was planned to follow an Agile-inspired @Agile101 workflow, meaning that the work was to be divided into week-long "sprints" with iterative task refinement. Task prioritization and addition of tasks to the backlog were done at the end of the week before the weekly supervisor meeting. Task management involved tracking various states for each task (on the Kanban board @kanban), including "blocked", "to-do", "in progress", "in review", and "done".

== Git <Git-section>
During the development process, the version control system Git @git-version-control was planned to be utilized in conjunction with GitHub @github. Additionally, a Kanban board @kanban on GitHub was used for task management.

The project's standard workflow for Git and GitHub was planned to involve maintaining each feature within a dedicated branch @git-branch. GitHub’s Kanban board would also allow for associating branches with specific tasks. Moreover, acceptance criteria would be established for each task on the Kanban board and, once all criteria were met, a pull request would be created to merge the changes into the main branch. Before finalizing the merge, at least one other team member would be required to review the code and the newly implemented feature. This review process would serve both as a quality assurance measure and as an opportunity to provide feedback.

== Godot <P-Godot>
The project was planned to be developed using the Godot game engine @godot. Godot is a free, open-source game engine that employs a node-based system that enables modular and reusable component design. It officially supports C\# and GDScript @GodotFeatures. Furthermore, community-driven extensions, such as the GDExtension @GDExtension, expand language compatibility to languages such as C++ and Rust.

== Benchmarking and Performance <benchmarking-and-performance-ref>
When developing real-time applications such as simulations, video games, or other computer applications, maintaining responsiveness and stability during runtime is essential for the user experience. A common metric to measure the performance of any such application is Frames Per Second (FPS), which is the amount of rendered images (frames) that are displayed each second. Higher and consistent FPS is desirable for a stable experience, as well as reduced visual artifacts and improved system latency from user input to its representation on the display @nvidia_fps.

However, average FPS alone does not always provide a complete picture of performance. Instead, examining individual frame times (the time it takes to render a frame) reveals inconsistencies that average FPS can mask. Issues such as brief momentary lag at computation-heavy moments may be overlooked in average FPS values, while being detrimental to the user experience. Metrics such as 1% lows and 0.1% lows of FPS have become common @gamers_nexus_dragons_dogma_benchmark, to capture these worst-case scenarios. This corresponds directly to capturing the slowest (highest value) 1% highs and 0.1% highs of frame times @nvidia_frametimes @techreport_inside_the_second.

The disparities between the three values: the total frame time average, the 1% highs, and the 0.1% highs, are important. Reducing the disparities is crucial for an overall stable user experience. Gamers Nexus @gamers_nexus_youtube_fps_lows mentions that disparities between frames of 8ms or more start to become perceptible to the user.

Overall, the project was planned to emphasize maintaining consistent frame times, rather than high average FPS, more precisely:
- Keep the disparities between the frame time average, its 1% highs, and its 0.1% highs, to a maximum of 8ms.
- Maintain an average of 60 FPS (equivalent to a frame time average of \~16.7ms), when the program is not experiencing its frame time 1%, or 0.1% highs.

Benchmarking was planned to be performed on a dedicated benchmarking computer named PC-1 (see @pc-1-specs), but would occasionally be performed on other machines, even though their results may not directly reflect performance on PC-1. If benchmarks were performed on a different machine, it would be explicitly noted. Even so, performance comparisons before and after changes could still offer meaningful insights into a relative change on PC-1. These specifications of other machines are noted in @all-pc-specs.

== Tasks
This subsection outlines the key tasks identified during the planning phase of the project. 

=== Procedural Content Generation
All content in the program was planned to be generated procedurally. To ensure persistent galaxy generation and reproducibility, all procedural content was to be generated using a fixed seed. This would allow for consistent environments across sessions and provide predictability during benchmarking and testing.

=== Planets
It was decided that the planets should primarily be generated using different noise algorithms. The planets should also be constructed using the Marching Cubes algorithm and be aesthetically distinct from one another.

=== Solar Systems
Solar systems were planned to be procedurally generated using randomized parameters such as the number of planets, celestial body orbits, and their physical attributes (e.g., mass, rotation, size). To ensure stability, key parameters such as orbital radius, mass, and velocity were designated for manual fine-tuning. This approach would allow for the possibility of generating coherent systems in real-time.

=== Physics Simulation
An accurate gravity simulation was a central requirement for achieving realistic physical behaviors. Different gravity systems were planned to be utilized for different purposes. For example, the player system could utilize the built-in physics engine to simulate gravity on planets, while the celestial dynamics utilize a manual implementation that is better suited for them.

=== Galaxy
To meet the requirement of implementing a procedurally generated, explorable galaxy, several design approaches were considered. These included generating solar systems dynamically at runtime to enable seamless exploration, as well as developing a galaxy map interface allowing players to select and explore individual solar systems via point-and-click navigation. It was also planned to apply the physics simulation to the entire galaxy.

=== Exploration
The ability to explore the generated content needed to be implemented. Multiple solutions were discussed, but since the main purpose was not to provide an engaging gameplay experience, it was decided that a simple camera controller would be prioritized, with the option to add more complex features if time allowed for it. Additionally, functionality for inter-solar system traversal needed to be implemented to allow for full exploration possibilities of multiple solar systems.

=== Optimization
The aim was to construct a real-time application, and thus, optimization techniques were of increasing importance. Inefficient algorithms and resource management could eventually lead to performance bottlenecks. To address this, proven techniques and smart solutions were planned to be explored throughout all parts of the project.

== Features <features>
After specifying the main objectives and implementation considerations, the project's features were identified and prioritized using the MoSCoW (Must have, Should have, Could have, Won't Have) analysis method @moscowprio:2018 (see @MosCowFinished). This was done to better understand which features to prioritize and which to save for later.

= Process <process-ref>
This section outlines the process for creating the various components that comprise the project. Each subsection represents a step in increasing scale, starting from the planet-scale, focusing on unique terrain generation and other planetary features, expanding to the system-scale organization of celestial bodies and their orbital physics, and finally reaching the galaxy-scale distribution of stars.

== Planet Generation <planet-gen-ref>
This section describes the planet generation implementation process.

=== Height-map planets <heightmap-planets>
The first planets to be constructed were the height-map planets. These planets were created by mapping a cube onto a sphere and displacing the vertices using height-map values (@height-maps) to create variation in the terrain elevation. Additionally, a simple planet shader was implemented to color the planets based on their height relative to their lowest point (see @fig:heightmap-planet). 

While height-maps were sufficient for generating simple planets, generating more complex terrain, such as overhangs or caves, required a different approach to be implemented. The Marching Cubes algorithm @marchingcubes:1998 was chosen for this purpose during the planning stage.

#figure(
  align(center,
    grid(
      columns: 2,
      gutter: 30pt,
      image("heightmap-planet.png", width: 160pt, height: 160pt),
      image("heightmap-planet-wireframe.png", width: 160pt, height: 160pt)
  ),
  ),
  caption: [Height-map planet with shader (left) along with its faces (right)]
)<fig:heightmap-planet>

=== Transitioning from height-maps to Marching Cubes <transition>
When transitioning from height-maps to Marching Cubes the method of generating the planets needed to change from a cube mapped onto a sphere to an isosurface, with each point containing a scalar values, as shown in @mct1.

The Marching Cubes algorithm was used to implement this approach, following the method outlined in @marching-cubes. @mct2 shows the result of this process applied to the scalar field shown in @mct1.

#align(center,
  grid(
    columns: 2,
    gutter: 30pt,
    [
      #figure(
        image("images/MarchingCube/mct1.png", width: 160pt, height: 140pt),
        caption: flex-caption(
          short: [Scalar field input for Marching Cubes
          ],
          long: [
            Scalar field input for Marching Cubes, where green represents inside the isosurface and red outside
          ]
        )
      )<mct1>
    ],
    [
      #figure(
        image("images/MarchingCube/mct2.png", width: 160pt, height: 140pt),
        caption: [Marching Cube generated mesh from the scalar field]
      )<mct2>
    ]
  )
)

=== Noise planets <noise-planets>

The noise planets were the first version of planets utilizing Marching Cubes. These planets were formed by generating a scalar field as shown in @mct1. Initially, the scalar values given to each point were binary in nature, with `-1` representing that point as being "outside" the surface ("empty space") and `1` representing the point being "inside" the surface ("included in the surface geometry"). To create spherical planets, points within a defined radius from the center were assigned the value `1`, while others were set to `-1`:

```cs
  distanceToCenter = (centerPoint - currentPosition).Length();
  if (distanceToCenter < radius) {
      points[x, y, z] = 1.0f;
  } 
  else {
      points[x, y, z] = -1.0f;
  }
```

This method produced a spherical planet as shown in @fig:noise-planet-1. However, the generated terrain lacked surface variation, which was addressed by introducing noise (@background-noise-ref). Instead of assigning constant values for the points inside the planet, a 3D noise function was used.

To give variation between generated planets, a random offset was also introduced when sampling the noise to avoid generating two identical planets. The code below shows how this was implemented and the result is demonstrated in @fig:noise-planet-2:

```cs
  if (distanceToCenter < radius) {
      points[x, y, z] = GetNoise3Dv(currentPosition + offset);
  }
  else {
      points[x, y, z] = -1.0f;
  }
```

#align(center,
  grid(
    columns: 2,
    gutter: 20pt,
    [
      #figure(
        image("images/PlanetNoise/noise_planet_sphere.png", width: 160pt, height: 160pt),
        caption: [Spherical planet generated using constant binary values]
      )<fig:noise-planet-1>
    ],
    [
      #figure(
        image("images/PlanetNoise/image.png", width: 160pt, height: 160pt),
        caption: [Spherical planet generated using 3D Perlin noise],
      )<fig:noise-planet-2>
    ]
  )
)

Although using noise made the terrain more visually interesting, the planet shown in @fig:noise-planet-2 did not appear as visually appealing as the earlier height-map planets. This was partly due to the planets being constrained to a perfect sphere, with all detail "carved out from the surface of the planet".


=== Interpolation
The planet surfaces shown in @fig:noise-planet-1 and @fig:noise-planet-2 appeared rough, despite being spherical. This roughness was due to the lack of interpolation in both the scalar field and the Marching Cubes algorithm.

The scalar field interpolation was implemented by using the distance from the planet's surface as the scalar value, rather than binary values. This resulted in a smoother transition between points located inside and outside the planet. The implementation can be seen in the pseudo code below:

```cs
  distanceToCenter = (centerPoint - currentPoint).Length();
  points[x, y, z] = radius - distanceToCenter;
```

Interpolation in the Marching Cubes algorithm was addressed by calculating where along the edges of the cube to place each point, based on the scalar values of the two points along the edge (see @fig:marching-cubes-interpolation). Previously, each point was only placed in the middle of the edge (see @mc15) which caused the mesh to become rough. 

The result of both of these improvements is shown in @fig:noise-planet-interpolation.

#align(center,
  grid(
    columns: 1,
    gutter: 20pt,
    [
  #figure(
      image("image445859.png", width: 300pt, height: 160pt),
       caption: [Visualization of internal interpolation in the Marching Cubes algorithm])
       <fig:marching-cubes-interpolation>
    ],
    [
  #figure(
     image("smooth_planet.png", width: 160pt, height: 160pt),
     caption: [Smooth spherical planet with interpolation])
     <fig:noise-planet-interpolation>
    ]
  )
)



=== fBm planets <fbm-planets>
The noise planets (@noise-planets) were a step in the right direction, but they lacked surface variation and resembled carved-out spheres. The initial idea to address this was to simply apply a height-map to all vertices as described in @heightmap-planets. However, this would potentially require three separate passes of the planet data: once to generate the data points, once during the Marching Cubes algorithm and finally, one more time to loop over all vertices of the newly created mesh. Furthermore, this approach would also eliminate the possibility of generating overhangs and more complex terrain features above the ground.

To address these issues, fractional Brownian motion (fBm) @fbm-chow, or simply fractal Brownian motion, was introduced. It is a technique used, for example, in the computer graphics and video games industry for generating realistic-looking terrain @fbm-chow, @quilez2019fbm. The idea is to layer several "octaves" (layers) of noise, each with increasing frequency and decreasing amplitude to produce smaller and smaller details. 

Consider a sine wave: if one were to add another sine wave to the first one, the amplitude of both would be added together. If the second sine wave had a larger frequency than the first but a lower amplitude, then its "height contribution" would become smaller and would add smaller detail to the end result. @fig:octaves shows the resulting graphs from the code below:

```cs
  firstOctave = sin(x);
  secondOctave = sin(x) + sin(2*x) * 0.5;
  thirdOctave = sin(x) + sin(2*x) * 0.5 + sin(4*x) * 0.25;
```

#figure(
  
align(center,
  grid(
    columns: 3,
    gutter: 55pt,
    grid.cell([
        #image("desmos-octave0.png", width: 150%)
    ]),
    grid.cell([
        #image("desmos-octave1.png", width: 150%)
    ]),
    grid.cell([
        #image("desmos-octave3.png", width: 150%)
    ])
  )
),
caption: flex-caption(
  short: [First octave, second octave and third octave],
  long: [First octave, second octave and third octave, created using Matplotlib @Hunter:2007]
)
)<fig:octaves>

The lower octaves have the same appearance as the higher octaves but on a smaller scale, due to fBm using fractals and being self-similar, which means that lower octaves have the same appearance as the higher octaves at certain scales @selfsimilarFractals. This property is useful when generating natural-looking terrain because it ensures that the detail will scale consistently and that the result will blend well together.

The implementation of fBm in this project was straightforward, and instead of using sine waves, 3D noise was used:

#figure(
    ```cs
  Fbm(valueToModify, pos, noise) {
    for (int i = 0; i < octaves; i++) {
        valueToModify += noise.GetNoise3Dv(frequency * pos + offset) * amplitude;
        amplitude *= persistence;
        frequency *= lacunarity;
        offset += new Vector3(random.Next(octaves));
    } 
    return valueToModify;
  }
    ```,
  caption: [Implementation of fBm],
)<fbm-code-cs>

The fBm function was applied using the distance to the planet’s surface as input, as this value is positive inside the planet and negative outside:

```cs
  distanceToCenter = (centerPoint - currentPosition).Length();
  distanceToBorder = radius - distanceToCenter;
  points[x, y, z]  = Fbm(distanceToBorder, currentPosition, noise);
```

In the fBm function (@fbm-code-cs), there is a single for-loop which, for each octave, transforms the previously used scalar value by adding noise with increased frequency and decreased amplitude. Lacunarity is the factor by which the frequency should be multiplied each octave, and persistence is the factor by which the amplitude should be multiplied each octave. Typical values for these are ```typst lacunarity = 2``` and ```typst persistence = 0.5``` when generating natural-looking terrain @quilez2019fbm. To produce more varied terrain (not strictly realistic), both of these parameters were chosen randomly based on the logic described in @proc-gen.

Using fBm, the planets were improved further, with more varied terrain and, most importantly, variation in the elevation. The result of these changes is demonstrated in @fig:fbm-1 and @fig:fbm-2:

#align(center,
  grid(
    columns: 2,
    gutter: 55pt,
    grid.cell([
      #figure(
        image("images/PlanetNoise/fbm_planet_1.png", width: 160pt, height: 160pt),
        caption: [fBm planet],
      )<fig:fbm-1>
    ]),
    grid.cell([
      #figure(
        image("images/PlanetNoise/fbm_planet_2.png", width: 160pt, height: 160pt),
        caption: [fBm planet with flat area],
      )<fig:fbm-2>
    ]),
  )
)

There were some issues with these planets, however, as shown in the center of @fig:fbm-2, where sometimes the scalar values at the edges got too large, cutting off areas prematurely, causing flat areas on the planet surface and occasionally square planets.

This issue was solved by introducing a fall-off parameter that reduced values closer to the edge:

```cs
  falloffRatio = distanceToCenter / radius;
  falloff = falloffRatio * falloffRatio * falloffStrength;
  points[x, y, z] = Fbm(distanceToBorder, currentPosition, noise) - falloff;
```

The optimal fall-off strength had to be fine-tuned manually by balancing the frequency of flat areas with the planet's reduced size when using a large fall-off strength. Although this solution did not completely eliminate the problem, it significantly reduced both the frequency and severity of the flat regions.

@fig:fbm_falloff_0 depicts a particularly severe instance of the discussed problem and @fig:fbm_falloff_8 and @fig:fbm_flaoff_32 demonstrate how the planet in @fig:fbm_falloff_0 transformed with different values of the fall-off strength. The method used to generate the planet's colors is explained later in @color-fbm-planets.

#align(center,
  grid(
    columns: 3,
    gutter: 55pt,
    grid.cell([
      #figure(
        image("fbm_planet_no_falloff.png", width: 160pt, height: 160pt),
        caption: [fBm planet without fall-off],
      )<fig:fbm_falloff_0>
    ]),
    grid.cell([
      #figure(
        image("fbm_planet_8_falloff.png", width: 160pt, height: 160pt),
        caption: [fBm planet with fall-off strength 8],
      )<fig:fbm_falloff_8>
    ]),
    grid.cell([
      #figure(
        image("fbm_planet_32_falloff.png", width: 160pt, height: 160pt),
        caption: [fBm planet with fall-off strength 32],
      )<fig:fbm_flaoff_32>
    ])
  )
)

=== Procedural planet generation <proc-gen>
The next step after creating the planet generation was to procedurally generate the planets at runtime. This was done by manually experimenting with the fBm parameter values until ranges that produced visually satisfactory results were identified. Then, the parameters were randomized, according to the logic presented in the following pseudocode:

```cs
  void RandomizeParameters() {
      Random random = new Random(planetSeed);
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

However, this method was non-deterministic, causing planets to be generated differently on subsequent visits. To fix this, planets were generated using a seed derived from its solar system's seed (as later described in @seed-ref).

=== Coloring the fBm planets <color-fbm-planets>
The code for coloring the planets was reused from the first planet implementation, with one extension. Cliff edges could be simulated by calculating the dot product between the direction of a specific vertex normal and the direction to the planet center. If the resulting value was close to one, the corresponding fragments at that position were assigned the cliff color (see  @fig:cliff-face). Some color themes were created to get aesthetically pleasing results (see examples in @PlanetColors).

#figure(
  image("images/Planet/cliff_faces.png", width: 50%),
  caption: [Cliff faces colored in dark gray]
)<fig:cliff-face>

#figure(
    grid(
      columns: 3,
      column-gutter: -100pt,
      [#image("images/Planet/iceWorld.png", width: 50%) a) Ice world],
      [#image("images/Planet/earthWorld.png", width: 50%) b) Forest world],
      [#image("images/Planet/lavaWorld.png", width: 50%) c) Lava or Red Desert world]
    ),
    caption: [A few select color themes available when generating the planets.]
) <PlanetColors>

Each theme was assigned a warmth value in the range [0, 1] (0 for coldest, 1 for warmest) and grouped by these values. This facilitated temperature-based coloring, where a planet's color theme was determined by its distance from its sun, using the following normalized warmth formula to simulate temperature falloff:

$
"normalized_warmth"=("planet_warmth" - "min_warmth")/("max_warmth" - "min_warmth")
$

== Optimizing the Planet Generation <planet-optimize-ref>
Replacing height-maps with Marching Cubes in mesh generation significantly increases computational demands due to the added dimensionality. This section describes how the planet generation was optimized to address this challenge.

=== Compute Shader
The first optimization step involved moving the Marching Cubes from the CPU to a compute shader on the GPU. Unlike the standard rendering pipeline, compute shaders are standalone programs invoked directly by the CPU, with custom-defined inputs @openglcs.

The main motivation for this shift was to leverage the GPU's strong parallelization capabilities. Given that the Marching Cubes processes each scalar independently, it inherently lends itself to parallel execution, making the GPU well-suited for this task.

After transitioning the Marching Cubes algorithm to a compute shader, this GPU-based mesh generation approach was tested against its CPU counterpart. The testing involved feeding identical scalar fields to both the CPU and GPU implementations of the algorithm and measuring the time required to generate the resulting mesh. The tests were performed across a range of different sizes to ensure broader applicability of the results.

Surprisingly, the GPU approach performed worse (see @compute-shader-performance-table). This was presumably due to overhead from buffer setup and data retrieval from the buffer, a common bottleneck in compute shader workflows. Additionally, the triangle buffer was configured for the worst-case polygonization (up to five vertices per polygon(s)), which added to retrieval time.

In the end, this approach did not achieve the intended reduction in planet generation time; however, as alternative methods remain available, the focus shifted to exploring a different solution.

#figure(
  table(
  columns: (auto, auto, auto),
  inset: 7pt,
  align: left,
  table.header([*Scalar field size*],[*CPU*], [*GPU*]),
  [32x32x32], [1.34ms], [98ms],
  [48x48x48], [3.33ms], [116.5ms],
  [64x64x64], [8ms], [172.67ms]
  ),
  caption: [Average time to generate vertices on PC-4 (@pc-4-specs)],
) <compute-shader-performance-table>


=== Worker Thread Pooling <worker-thread-pooling-ref>
An alternative approach involved distributing the workload across multiple threads. In addition to the main thread, a dedicated thread was introduced to handle planet generation requests. Previously, all operations, including the loop that generated each planet during solar system creation, ran on the main thread. This caused performance issues, as the generation process led to noticeable stuttering; the frame could not advance until all meshes were fully generated. By offloading the generation tasks to a separate thread, the main thread could dispatch requests and proceed without delay.

To further optimize the two-threaded approach, a thread pool @threadpool2 was introduced to distribute planet generation across multiple threads instead of relying on a single one. A thread pool pre-allocates a set number of threads at startup; tasks such as planet generation are queued and handled by available workers. This improves performance by reusing threads and avoiding the overhead of constantly creating new ones.

This multi-threaded adjustment reduced stuttering between frames and allowed for a smoother experience (see @worker-thread-pool-frame-time).

#figure(
  table(
  columns: (auto, auto, auto, auto),
  inset: 7pt,
  align: left,
  table.header([],[*Average*], [*1% high*], [*0.1% high*]),
  [From], [4,21 ms], [8,55 ms], [40,1 ms],
  [To], [4,2ms], [7,73 ms], [33,74 ms],
  ),
  caption: [Frame time metrics on PC-4 (@pc-4-specs) - Worker Thread Pool],
) <worker-thread-pool-frame-time>

=== Chunking & Level-of-detail <Octree-Planet>
A chunking system was implemented to optimize planet generation. To further boost performance, a level-of-detail (LOD) mechanism was added, reducing mesh complexity for distant planetary regions by using fewer scalar values, which is especially important when using the Marching Cubes algorithm. This speeds up loading as rendering less detailed areas is quicker.

The implementation works by having an _Octree_ class, which is initialized with a size matching the planet's diameter, while the _OctreePlanetSpawner_ class manages mesh generation. A resolution variable sets the number of scalar values per chunk (e.g., $32^3$ for a resolution of 32). As the player nears a leaf node, it subdivides, unless it is at a predefined max depth (to avoid infinite subdivisions), generating higher-resolution meshes while disabling the parent mesh. Moving away reverses this process, with subdivisions removed and the parent mesh restored. The root node always remains intact. This dynamic adjustment ensures that only nearby regions are rendered in high detail, improving efficiency.

However, a limitation of the Marching Cubes algorithm with varying LODs is occasional gaps between chunks of different resolutions (as partially evident in @fig:octmc2), as higher-resolution chunks capture more surface points while lower-resolution might miss them. 

There are several possible solutions to address this issue. One approach involves the use of a skirt, which mitigates the problem by layering vertices along the edges of a chunk, effectively creating a surrounding skirt. Another method is stitching, where, in cases of adjacent chunks with differing LODs, the edges of neighboring chunks are connected to ensure continuity. A more advanced solution is the Transvoxel algorithm @Transvoxel2010, specifically designed to handle transitions between chunks with varying LODs. However, due to time constraints, none of these solutions were implemented, and the issue remained unresolved.

#align(center,
  grid(
    columns: 2,
    gutter: 10pt,
    grid.cell([
      #figure(
        image("images/Octree/octmc1.png", width: 160pt),
        caption: [Octree planet outlining the chunks],
      )<fig:octmc1>
    ]),
    grid.cell([
      #figure(
        image("images/Octree/octmc2.png", width: 160pt),
        caption: [Octree planet showing the varying LODs],
      )<fig:octmc2>
    ]),
  )
)

== Planetary Features
This section describes the process of creating the additional planetary features, including surface details, surface features, oceans, and atmospheres.

=== Surface Details<p-surface-details>
Surface details, such as grass, are classified in _Population of a Large-Scale Terrain_ @surfacedetails1 as elements rendered in close proximity to the player. Their positioning is not deterministic due to their high density and the player's limited attention to individual instances. 


To generate surface details, the system first identifies the current chunk the player is in, along with adjacent chunks within a defined range. It then iterates through all triangles in the mesh data of each relevant chunk. For each triangle, a random point is generated within its bounds using barycentric coordinates @barycentriccoordiantes1.

Given a triangle with vertices $bold(a)$, $bold(b)$, and $bold(c)$, and two random variables $r_1$ and $r_2$, a random point $bold(d)$ within the triangle can be computed using the following formula @barycentriccoordiantes1:
$ bold(d) = (1 - sqrt(r_1))bold(a) + sqrt(r_1)(1 - r_2) bold(b) + sqrt(r_1) r_2 bold(c) $ 
This method ensures that the point $bold(d)$ lies uniformly within the triangle. To determine the orientation of the surface detail, the normal vector of the triangle is calculated via the cross product of its edge vectors:
$ bold(n) = (bold(b) - bold(a)) crossmark (bold(c) - bold(a)) $
The resulting normal vector $bold(n)$ is then normalized. An orthonormal basis is constructed using the normal as the y-axis. The x-axis vector is chosen based on the vertical component of the normal to maintain numerical stability:
```cs
  Vector3 upVector = normal;
  Vector3 xVector;
  if (Mathf.Abs(upVector.Y) < 0.99f)
    xVector = new Vector3(0, 1, 0).Cross(upVector).Normalized();
  else
    xVector = new Vector3(1, 0, 0).Cross(upVector).Normalized();
```
The z-axis vector is obtained as the cross product of the x and y vectors, forming a complete orthonormal basis used to orient surface details.

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
An initial issue encountered was that only a single surface detail was rendered per face (see @fig:simplegrass1 and @fig:simplegrass2), making the total number of rendered instances dependent on the face count of the mesh. 

To address this, a density parameter was introduced to control the desired number of instances. In order to distribute the instances proportionally across the mesh, the total mesh area is first computed and then multiplied by the density value to determine an overall target number of instances.
For each face, the fraction of the total area is multiplied by the total target to obtain an “ideal instance count.” 

In certain cases, the combination of area and density parameters may yield an instance count of less than 1. To address this, when the instance value is below 1, a random check decides whether to add a single instance or none. Otherwise, the count is rounded to an integer. This approach distributes instances proportionally to each face’s area.

```cs
  int totalInstanceTarget = Mathf.Max(1, Mathf.FloorToInt(totalArea * density));

  float faceAreaRatio = faceArea / totalArea;
  float idealInstanceCount = totalInstanceTarget * faceAreaRatio;
  
  int faceInstances;
  if (idealInstanceCount < 1.0f)
  {
    // Probabilistic approach - chance equals the fractional ideal count
    faceInstances = GD.Randf() < idealInstanceCount ? 1 : 0;
  }
  else
  {
    // Round to nearest integer instead of floor for better distribution
    faceInstances = Mathf.RoundToInt(idealInstanceCount);
  }
```

By iterating through each face and applying this calculation, the number of instances is determined and their positions and orientations are computed accordingly. With the density parameter set to 10, the resulting distribution is shown in @fig:simplegrass3.

#grid(
  columns: 2,
  gutter: 0.75cm,
  grid.cell([
    #figure(
    image("images/Grass/grassfig5.png", width: 200pt),
    caption: [Density-Based Grass Spawning]
  )<fig:simplegrass3>
]),
  grid.cell([
    #figure(
    image("images/Grass/grassfig5_wireframe.png", width: 200pt),
    caption: [Wire-frame of Density-Based Grass Spawning]
  )<fig:simplegrass4>
])
)


Applying a grass shader to the grass blade meshes resulted in enhanced surface detail on the planet, as demonstrated in the final visual output.
#figure(
  image("images/Grass/grass7.png", width: 300pt),
  caption: [Grass on a planet]
)

=== Surface Features <p-surface-features>
Surface features, such as trees, are classified in _Population of a Large-Scale Terrain_ @surfacedetails1 as elements that must remain visible from a distance and require consistent placement. Positional irregularities in these features are easily noticeable and can disrupt visual coherence.

Unlike surface details, surface features must remain visible at greater distances and cannot rely on dense mesh data, which is incompatible with the octree-based terrain system. The reduced triangle count at lower octree resolutions renders geometry-dependent techniques ineffective and inconsistent.


To address this, an alternative approach based on ray-casting was implemented. Rays are cast inward from randomly sampled points on the planet's axis-aligned bounding box (AABB). The AABB is a rectangular box that encompasses an object, aligned with the coordinate axes (x, y, z). Upon terrain intersection, hit positions and surface normals are extracted to procedurally instantiate surface features using a construction pipeline similar to Surface Detail but decoupled from mesh density.

Points on the AABB are generated using uniform random sampling with bilinear interpolation across each face with the following formula:

#align(center)[$bold(p) = bold(v_1) + u (bold(v_2) - bold(v_1)) + v * (bold(v_4) - bold(v_1))$]

where  $v_1$, $v_2$, and $v_4$ are the three vertices defining an AABB face, and $u$ and $v$ are random values in the range $[0,1]$. This assumes rectangular faces, which is true for AABBs.

#grid(
    columns: 2,
    gutter: 0.75cm,
        grid.cell([
      #figure(
        image("images/SurfaceFeatures/points_1.png", height: 160pt),
        caption: [The AABB (marked yellow) encapsulating the planet],
      )<fig:points1>
    ]),
    grid.cell([
      #figure(
        image("images/SurfaceFeatures/points_2.png", height: 160pt),
        caption: [Randomized points on the AABB]
      )<fig:points2>
    ])
)

Although this uniform sampling technique is computationally efficient and straightforward to implement, it tends to produce uneven spatial distributions. Specifically, it may result in clustering of points in some regions and sparse coverage in others—an outcome that is undesirable when attempting to ensure consistent surface feature placement across the planetary surface (see #ref(<randomvspossion>)).

#figure(
  grid(
  columns: 2,
  gutter: 1.5cm,
  grid.cell([#image("images/SurfaceFeatures/poisson.png", height: 160pt)]),
  grid.cell([#image("images/SurfaceFeatures/uniformpoints.png", height: 160pt)])
  ), caption: [Poisson (left) vs Uniform (right) distribution]
)<randomvspossion>

To achieve a more spatially uniform distribution of points, the sampling method was subsequently replaced with Poisson-disc @bridson2007fast sampling. Unlike uniform sampling, Poisson-disc sampling ensures that each point is separated by a minimum distance $r$, thereby avoiding clustering and producing a more even distribution of samples.
The algorithm follows the method described in Fast Poisson Disk Sampling in Arbitrary Dimensions @bridson2007fast. It operates as follows, with the input parameter _r_ representing the minimum distance and the constant _k_ indicating the number of samples before rejection:

*Step 1. * Initialize a background grid with cell size $frac(r, sqrt(n))$ where $n$ is the dimensionality of the space (in this case, $n = 3$). 

*Step 2. * Randomly select an initial sample within the domain and insert it into both the active list and the background grid.

*Step 3. * While the active list is non-empty:
#list(indent: 1cm, spacing: 1em, 
  [Randomly choose a sample from the active list.],
  [Generate up to $k$ candidate points within an annulus of radius $r$ to $2r$ around the chosen sample.],
  [For each candidate:
    - Reject it if it lies outside the domain or is closer than $r$ to any existing sample.
    - If it is valid, add it to both the active list and the grid.],
  [If none of the $k$ candidates are accepted, remove the sample from the active list.]
)

After establishing a more spatially uniform distribution of points, the next step involved determining an effective method for distributing varied environmental features such as large trees, small trees, and rocks. 

One effective approach for achieving this distribution is the Alias method @aliasvose. The Alias method enables sampling from a discrete probability distribution in constant time. Given a list of $n$ probabilities, it allows for the selection of an index i, where $1 <= i <= n$, according to the specified distribution. 

For example, consider a case where three types of features are to be placed in a scene: large trees, small trees, and rocks. Suppose the desired probabilities for selecting each feature are as follows:

- Large trees: 50%
- Small trees: 30%
- Rocks: 20%

Using the Alias method, this distribution is encoded into two tables—one for probabilities and one for aliases. At runtime, a uniformly random index is selected, and the final outcome is determined using the stored values, thereby ensuring that features are placed according to the specified probabilities in an efficient manner.

#figure(
  grid(
    columns: 2,
    gutter: 0.1cm,
    grid.cell([
      #image("images/SurfaceFeatures/treesscr.png", width: 95%)
    ]),
    grid.cell([
      #image("images/SurfaceFeatures/treesscr2.png", width: 95%)
    ])
  ), 
  caption : flex-caption(
    short: [Surface features on a planet.],
    long: [Surface features on a planet. Tree asset by user _LOLIPOP_, via #link("https://sketchfab.com/3d-models/maple-trees-pack-lowpoly-game-ready-lods-b5d2833c258f4054a01ee2b4ef85adf0")[#underline[sketchfab.com]], licensed under CC BY 4.0.]
  )
)<fig:surfacefeatures1>

=== Oceans
The ocean layer was implemented as a simple blue sphere, expanded from the planet's center to some configurable radius (typically equal to or slightly less than the planet radius). This sphere then intersects with the planet terrain, creating an ocean. However, only having a smooth blue sphere as water was deemed not visually appealing, thus a water shader was implemented.

Surface motion was implemented using bump mapping @unityNormalMapBumpMap2024, scrolling two separate noise textures in different directions on top of the water surface, and using them as normal maps @unityNormalMapBumpMap2024 to displace each vertex normal during lighting calculations, creating visual "bumps" on the surface. To add actual wave movement, displacement mapping @RTR4 was used to physically alter the vertices of the mesh using another noise texture as a height-map. @fig:water-ball shows the result of both of these techniques.

One challenge with applying these techniques to a sphere (rather than flat terrain) is texture seams and pinching at the poles (see @fig:water-pinching). Godot's support for seamless noise resolved the seam issue. To address polar pinching, one solution was found: triplanar mapping @TriplanarMapping. However, since the pinching is mostly hidden by terrain and rarely visible (and in the interest of time), the issue was considered acceptable and left unaddressed. Triplanar mapping was later implemented in @moons-ref but was not applied to the ocean, since it used a mesh without built-in support.

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

Another glaring issue was flickering due to the ocean and the atmospheres (@atmospheres) clashing. This was resolved by making the atmosphere have a higher render priority than the ocean layer, meaning that the ocean gets rendered before the atmosphere.

The oceans looked cohesive with the planets, so the next step was to make the water transparent and to color the water based on its depth. This was achieved by using a combination of the depth texture @godotDocsDepthtex to calculate the distance of each pixel to the water surface, and the screen texture @godotDocsScreentex to blend the planet's surface color with the water color. The water color was also divided into two separate colors, one brighter color for shallow parts and one darker color for deeper parts. Using the Godot built-in function for blending two colors @godotDocsMix, the final per-pixel water color was calculated:

```cs
  vec3 water_depth_color = mix(deep_color, shallow_color, smooth_shallow_depth);
  vec3 transparent_water_depth_color = mix(screen * smooth_shallow_depth,   
  water_depth_color, transparency_blend);
```

Finally, the color around the edges of the ocean (where the ocean surface meets the terrain), was colored white to appear as foam. This was done by blending the previously calculated color with a foam color:

```cs
  vec3 transparent_water_depth_color_with_foam = mix(transparent_water_depth_color, 
  foam_color, smooth_foam_depth);
```

Put together, the ocean greatly improved the visuals of the planets. The result is demonstrated in @ocean-shader.

#figure(
  image("oceans-new-picture.png", width: 160pt, height: 160pt),
  caption: [Planet with ocean and water shader]
) <ocean-shader>

=== Atmospheres <atmospheres>
Atmosphere development began with research into existing solutions, resulting in two approaches: a simpler method by Martin Donald @martin-donald-atmosphere and a more physically accurate one by Sebastian Lague @sebastian-lague-atmosphere. Both implementations utilize a post-processing shader on a cube with flipped faces.

The simpler version was implemented using ray-sphere intersections to create a transparent, uniformly colored, sphere around the planet (see @ray-sphere-atmosphere). The color remains uniform since it does not account for the sun's position. Attempts to improve shading involved calculating the dot product between each vertex normal and the sun ray directions, meant to influence the resulting color. However, due to issues with different coordinate spaces and fetching the sun's position, this was unsuccessful and caused a shift towards the more realistic approach.

#figure(
  image("images/Atmosphere/basic_atmosphere.png", width: 50%),
  caption: [Implementation of a simple atmosphere]
) <ray-sphere-atmosphere>

The second iteration aimed for a more physically accurate atmosphere involving Rayleigh scattering. Rayleigh scattering is a physical phenomenon that describes how light interacts with particles smaller than its wavelength @RayleighScattering, allowing for atmospheric light scattering, density falloff with altitude, and sunsets. The algorithm roughly works by approximating light scattering along a ray cast from the camera through the atmosphere, using a series of sampling points for optical depth and scattering calculations.

The color is based on a 3D vector representing different light wavelengths corresponding to the different parts of the visible light spectrum. However, basing it on only Rayleigh scattering limited the achievable color options. For example, a red Mars-like atmosphere was not possible through this method alone. To address this, one of the wavelengths was set lower than the others, amplifying the scattering of that wavelength, and thereby altering its color. Finally, a set of preset wavelength vectors were created and are chosen at random as the planets are generated. See @AtmosphereColors.

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

Initially, atmospheric rendering was performance-heavy due to costly light scattering calculations. Two solutions were considered: offloading calculations to a compute shader, and implementing a LOD system. The latter was chosen for its simplicity, and is done by dynamically reducing scattering and optical depth samples with distance to the player.

Originally set to 30, the number of sampling points along the rays traveling through the atmosphere caused significant performance drops. After testing, a sample count of 10 was found to balance visual fidelity and performance effectively, reducing average frame time from 132.2 ms (at 30 samples) to 18.56 ms in a single-planet benchmark on PC-3 (see #ref(<pc-3-specs>)). Furthermore, with the LOD in place, the amount of sampling points reduces from the set maximum value, downwards to 1, as the distance to the player increases. This further increases performance since not all atmospheres in a system have to render at full quality at the same time.

== Player Controller <player-controls-ref>
The player controller was initially implemented as a flying camera for free exploration of the galaxy, without collision or surface interaction. After terrain generation was completed, support for planetary landings began its development. This required simulating local gravity, surface-aligned movement, and jumping.

Planetary gravity fields were implemented using an Area3D node @GodotArea3D with a spherical collision shape (see #ref(<Area3D>)). This node allows any physics body that enters its collision shape to inherit the gravity direction and strength set by the Area3D. The gravity direction is calculated by subtracting the camera's world position from the planet's world position. This is updated during each physics step to allow the direction to always point towards the planet center.

#figure(
  image("images/Planet/Area3D.png", width: 50%),
  caption: [Area3D node with collision shape around a planet]
)<Area3D>

To facilitate exploration and simulate orbital behavior, the player inherits a planet’s total velocity upon entering its gravitational field, ensuring they remain in orbit. The final step was implementing surface landing and movement mechanics.

Rotating the player while moving on the planetary surfaces was achieved by linearly interpolating the player’s basis toward a target basis with its ‘up’ vector aligned against the gravity vector. Finally, the ability to jump was implemented by adding an impulse along the opposite direction of the gravity vector.

Several issues emerged during testing. At high velocities, the player could fall off planets, which was resolved by lowering the base speed. Uncontrollable bouncing, likely caused by uneven terrain at high speeds, was mitigated by adding a downward raycast to support the collision system. Furthermore, many issues were ultimately traced to planets moving at high speeds while the player was also in motion, combined with inaccurate planetary collision shapes.

== Physics Engine <physics-engine-ref>
Simulating the gravitational interactions within a galaxy, containing a vast number of stars and planets, presents a significant computational challenge known as the N-body problem @Gangestad2025. The goal is to calculate the net gravitational force acting on each body at discrete time steps and use this information to update their positions and velocities over time. This section details the progression of methods implemented to tackle this problem within our project, moving from a simple baseline to an optimized approximation algorithm, and discusses the performance analysis that guided these choices.

=== Direct Summation <physics-direct-summation-ref>
One approach to solving the N-body problem is the direct summation method. This technique relies directly on Newton's Law of Universal Gravitation @newton1687, calculating the gravitational force between every pair of particles in the system.

The force $arrow(F)_12$ exerted on particle 1 by particle 2 is given by:
$
  arrow(F)_12 = G (m_1 m_2)/(|arrow(r)_12|^3) arrow(r)_12 quad "where" quad arrow(r)_12 = arrow(r)_2 - arrow(r)_1
$

The total acceleration $arrow(a)_i$ on particle $i$ is the sum of accelerations caused by all other particles $j != i$ in the system, as illustrated in @fig:direct:
$
  arrow(a)_i = sum_(j != i) G m_j/(|arrow(r)_(i j)|^3) arrow(r)_(i j)
$

#figure(
  image("pairwise.png", width: 50%),
  caption: [Diagram illustrating pairwise force calculation between five bodies]
)<fig:direct>

This requires $N(N−1)/2$ pairwise calculations per time step, resulting in a computational complexity of $O(N^2)$. Our implementation used nested loops as illustrated in the pseudocode in @pseudo-direct.

While simple and accurate, the $O(N^2)$ complexity made direct summation computationally prohibitive for large N within the target performance goals, necessitating an approximation method.

=== Barnes-Hut Approximation
To efficiently simulate large numbers of bodies, the Barnes-Hut algorithm was implemented, reducing the computational complexity to $O(N "log"N)$ @Barnes_Hut_1986. The core idea was to use an octree @octree1 (described in @B-octree) to group distant particles together. The gravitational influence of these groups was then approximated by treating the group as a single point mass located at the group's center of mass (CoM). This approximation leveraged Newton's shell theorem @newton1687 and is effective when the distance to the group is large compared to the group's size.

In dynamic N-body simulations, constantly changing particle positions quickly invalidate the octree, typically requiring reconstruction each time step (physics frame). This demands an efficient construction algorithm, for which a *Morton-code-based linear octree* @gargantini1982 was chosen, enabling:

1.  *Parallelizable Steps:* Both calculating Morton codes and sorting particles are trivial to parallelize.

2.  *Cache Efficiency:* Processing spatially local data sequentially after sorting can lead to better CPU cache utilization compared to pointer-chasing in traditional octree implementations.

3.  *Efficient Partitioning:* The sorted order allows for fast partitioning of particles into child nodes using binary search rather than geometric tests.

The construction process proceeds as follows:

1. *Morton Codes:* A 64-bit Morton code is calculated for each particle by mapping a particle's 3D position within the global simulation bounds to a 1D integer by interleaving the bits of its scaled coordinates (see @fig:interleaving). This mapping largely preserves spatial locality since nearby particles tend to have numerically close Morton codes @morton1966. This step is parallelized using `rayon` @rayon for particle counts larger than a set threshold, determined via benchmarking (see @physics-benchmarking-ref).

2. *Sorting:* Particles are sorted based on their Morton codes, effectively grouping spatially adjacent particles in memory, as illustrated in @fig:morton-3d. The sorting is parallelized using `rayon`, providing significant speedup. 

3. *Tree Construction:* An explicit linear tree structure is built recursively from the sorted list of particles. The construction works by dividing the particle list into smaller ranges:
    - If a range contains one particle or the maximum depth is reached, a leaf node is created, and its `GravityData` (mass and center of mass) is computed.
    - For internal nodes, the range is partitioned into 8 sub-ranges (octants). This is done efficiently by performing a binary search on the sorted Morton codes, checking the relevant 3 bits at the current depth to find the split points.
    - Recursive calls are made for each non-empty octant, and parent nodes aggregate `GravityData` from their children.
  The tree-building step is run sequentially, but there's potential for parallelization.

#grid(
  columns: (1fr, 1fr),
  [
    #figure(
      image("morton_z_curve_2d.png", width: 94%),
      caption: flex-caption(
        short: [2D version of Morton codes, showing how interleaving works.
        ],
        long: [2D version of Morton codes, showing how interleaving works. Nomen4Omen, CC BY-SA 4.0]
      )
    ) <fig:interleaving>
  ],
  
  [
    #figure(
      image("morton_3d.png"),
      caption: flex-caption(
        short: [How points are ordered in 3D when sorted using Morton codes.
        ],
        long: [How points are ordered in 3D when sorted using Morton codes. Robert Dickau, CC BY-SA 3.0]
      )
    )<fig:morton-3d>
  ]
)

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

Once the octree is built for the current time step, each particle's acceleration is computed by traversing the tree, using the _Multipole Acceptance Criterion (MAC)_ @Barnes_Hut_1986. This criterion determines whether a node (a region of space containing particles) is sufficiently far from the target particle to be approximated as a single point mass. The principle is illustrated in @fig:mac, which uses a 2D quadtree for clarity; an octree applies the same logic in 3D by subdividing space into eight octants instead of four quadrants.

The MAC involves these steps during tree traversal for a target particle:
1.  Compute the distance $d$ between the target particle and the current node's center of mass (CoM).
2.  Determine the node's characteristic size $s$ (e.g., bounding box width).
3.  If $s^2 < theta^2 d^2$ (where $theta$ is a predefined threshold parameter, typically between 0.5 and 1.0), the node is considered "far enough." Its gravitational effect on the target particle is then approximated using the node's total mass and CoM, and the traversal down this branch of the tree stops.
4.  If the node is "too close" ($s^2 >= theta^2 d^2$):
    - If it is an internal node, the algorithm recursively traverses its non-empty children, applying the MAC test to each.
    - If it is a leaf node, direct summation (@physics-direct-summation-ref) is performed between the target particle and each particle within that leaf. A softening factor $epsilon$ is used in these calculations ($d^2$ is replaced by $d^2+epsilon^2$) to prevent numerical instability from particles being too close.

Because each particle's traversal is an independent tree traversal, this process is trivially parallelized using `rayon`, greatly improving performance:

#box[
```rs
  accelerations = (0..particles.len())
      .into_par_iter() // Only need this line to pararellize computation
      .map(|i| octree.calculate_accel_on_particle(g, i))
      .collect()
```
]

#figure(
  image("barnes-hut-MAC.png"),
  caption: flex-caption(
    short: [Illustration of the Barnes-Hut Multipole Acceptance Criterion (MAC)
    ],
    long: [Illustration of the Barnes-Hut Multipole Acceptance Criterion (MAC). Left: Spatial view showing a target particle and nested quadtree cells. The ratio of cell size (s) to distance (d) from the target particle to the cell's Center of Mass determines if the cell can be approximated. Right: Corresponding tree traversal. For each node, if $s^2/d^2 >= theta^2$ (MAC fail), the algorithm recurses to its children. If $s^2/d^2 < theta^2$ (MAC success), the node's entire mass is treated as a single point particle for force calculation.]
  )
)<fig:mac>

=== Integration <P-integration>
Finally, the calculated accelerations (whether from Direct Summation or Barnes-Hut) were used to advance the simulation state via numerical integration. The implementation used the Forward Euler method @euler1768integral (specifically, symplectic Euler, which offers better long-term stability for orbital mechanics @brorson_symplectic_integrators) to update velocities and positions based on the accelerations computed in parallel for the current time step $Delta t$:

#align(center)[
$v_"new" = v_"old" + arrow(a) dot Delta t quad $ (```rs body.vel += acc * delta```)

$p_"new" = p_"old" + arrow(v)_"new" dot Delta t quad $ (```rs body.pos += body.vel * delta```)
]

This combination of parallelized efficient octree construction via Morton codes, $O(N "log"N)$ force calculation with parallel traversals, and simple Euler integration allowed the simulation of large-scale galactic systems.

=== Performance Benchmarking and Threshold Tuning <physics-benchmarking-ref>
To guide optimization and inform decisions about algorithms and parallelization, rigorous performance benchmarking was conducted on the core components of the physics engine. The Rust library criterion @criterion.rs was used, offering statistical insights through repeated runs, regression detection, and detailed reports.

The benchmarks focused on performance under varying workloads, particularly different numbers of simulated bodies ($N$). Key tests compared acceleration calculations (e.g., Direct Summation vs. Barnes-Hut) and parts of the Morton-based octree construction. Consistent synthetic test data ensured reproducibility across runs.

1. *Algorithm Selection Thresholds for Force Calculation:* The key results for the Direct Summation vs. Barnes-Hut benchmark are summarized in @fig:calc-acc-bench. This graph plots the average computation time (in milliseconds, on a logarithmic scale) against the number of bodies for four variants: sequential Direct Summation (`direct/sequential`, yellow line), parallel Direct Summation (`direct/parallel`, blue line), sequential Barnes-Hut (`barnes_hut/sequential`, green line), and parallel Barnes-Hut (Morton-based octree, `barnes_hut/parallel`, red line). Each data point represents the average of 100 samples from benchmarks run on PC-1 (see @pc-1-specs). Since these benchmarks are hardware-specific, the established performance thresholds might vary on other systems, especially regarding the number of CPU cores. This visualization was crucial for determining these thresholds.

    - For very small numbers of bodies ($N < 100$), the `direct/sequential` method (yellow line) is the most performant. Its low intrinsic overhead makes it ideal for these scenarios, despite its $O(N^2)$ complexity. All other algorithms show an overhead, especially the parallel ones.
    
    - As $N$ increases beyond approximately $100$, the `direct/parallel` method (blue line) surpasses sequential direct summation and also remains faster than `barnes_hut/parallel` (red line) for a significant range. This is because the parallelization of the $N^2$ calculations effectively utilizes multiple cores, and this benefit outweighs the Barnes-Hut octree construction overhead until the $N^2$ factor becomes too dominant.
    
    - The final crossover occurs at approximately $N=440$, where the `barnes_hut/parallel` method (red line) becomes the most efficient. Beyond this point, the $O(N "log"N)$ complexity of Barnes-Hut, combined with parallelism, provides superior performance over both direct summation variants.

These results directly informed the dynamic switching system in the physics controller. Based on the number of bodies, the simulation adaptively selects the best-performing algorithm: sequential for few bodies, parallel direct sum for moderate counts, and parallel Barnes-Hut for large numbers. The 60 FPS threshold in the diagram illustrates their real-time viability.

#figure(
 image("calc_acc_bench.png"),
 caption: flex-caption(
   short: [Criterion benchmark results for N-body acceleration calculations
   ],
   long: [Criterion benchmark results for N-body acceleration calculations, illustrating the performance crossover points that informed the selection of algorithm-switching thresholds. Average time per 100 samples (ms, log scale) vs. number of bodies. Key thresholds for Direct Summation vs. Barnes-Hut, and parallelization overheads are noted.]
  )
)<fig:calc-acc-bench>

2.  *Parallel Morton Encoding Threshold:*
    This benchmark group specifically compared the performance of calculating Morton codes sequentially versus in parallel using rayon. The results are visualized in @fig:morton-bench. This graph plots average encoding time against the number of bodies, for both sequential and parallel implementations.

    
    As the diagram illustrates, for smaller numbers of particles, the sequential encoding is faster due to the overhead associated with initializing and managing parallel tasks. However, as the number of bodies increases, the benefits of parallel computation become apparent. The intersection point, where the parallel version starts to outperform the sequential one, is around $N approx 4000$. Based on these empirical results, for particle counts below 4000, Morton codes are calculated sequentially, while for counts at or above 4000, the parallel `rayon` implementation is utilized to leverage multi-core processing.

#figure(
  image("morton-bench.png"),
  caption: flex-caption(
    short: [Morton encoding criterion benchmark results
    ],
    long:[Morton encoding criterion benchmark results demonstrating the benefit of parallelization for larger datasets. Average time per 100 samples (ms, log scale) vs. number of bodies. Parallel encoding surpasses sequential performance around N ≈ 4000, after overcoming initial parallelization overhead incurred by rayon.]
  )
)<fig:morton-bench>

In summary, using `criterion` for systematic benchmarking was crucial for optimizing the physics engine. It provided the quantitative data necessary to justify algorithmic choices and fine-tune parameters such as parallelization and algorithm-switching thresholds, resulting in a more performant and scalable simulation.

=== Trajectory Simulation and Visualization <P-trajectories>
To understand orbital dynamics and aid in system design, a trajectory simulation system was implemented to predict and visualize the future paths of celestial bodies. This system runs a separate N-body simulation for a configurable number of future steps and time increment, using the same core physics logic (Direct Summation or Barnes-Hut) and semi-implicit Euler integration as the main simulation. The resulting sequences of future positions for each body are rendered as colored line strip meshes. Trajectories can also be calculated relative to a central body.

Given the computational cost, especially for many steps or bodies, trajectory calculations are offloaded to a background thread. The main thread sends messages to this worker and retrieves results asynchronously using a queue-and-poll mechanism. This prevents the main game loop from freezing during intensive calculations. The worker is designed to process the latest request if multiple are queued.

The accuracy of these predicted trajectories is subject to the same numerical errors as the primary Euler integration, accumulating with the number of steps; smaller time deltas improves accuracy at the cost of computation. However, it was observed that less precise trajectories (larger time delta) tended to overestimate instability, providing a useful heuristic: if a system appeared stable with coarse predictions, it was generally stable in practice.

This trajectory visualization proved invaluable for the System Generation process (@system-gen-ref), allowing for iterative tuning of orbital parameters to achieve stable or aesthetically desirable configurations. It served as a key diagnostic tool for debugging physics and visually confirming the immediate future dynamics of generated solar systems.

== System Generation <system-gen-ref>
System generation involved deterministically generating stable and aesthetically plausible solar systems and their contained celestial bodies. This included calculating orbital positions for planets and moons to ensure long-term stability, and assigning unique seeds to each celestial body for reproducible procedural generation.

The generation of systems should balance realism, aesthetics, and stability. Generating stable systems with realistic distances lead to planets appearing too small to be visible, which deteriorates the game play experience. On the contrary, smaller distances with large planets could lead to instability, specifically for moons. The moons' orbit radius increase with the planets' radius, increasing the risk of moons interacting neighboring planets.

To verify a system's stability, the previously implemented trajectories (see #ref(<P-trajectories>)) were used (see #ref(<threePlanetTrajectories>)). However, these trajectories were not entirely accurate; numerical errors accumulate over time, meaning that initially stable orbits may eventually become unstable.

These errors tended to grow when bodies undergo large transformations between physics steps. The errors were reduced by utilizing a higher physics frame rate and slowing down the orbit speeds. Conversely, faster orbits could lead to a higher accumulation of errors, making stable systems appear unstable in the simulation. 

#figure(
  image("trajectoriesSystem3Planets.png", width: 50%),
  caption: [Three planet system with visible trajectories.]
)<threePlanetTrajectories>

=== Solar System Stars
The stars were at first implemented as simple yellow spheres with constant masses and fixed positions and radiuses. Other than the colors, the constant variables would not be randomly generated as the stars would not physically interact with other stars.

To increase variety between solar systems and increase visual interest, a star shader snippet @StarShader was found online and implemented. Additionally, an array of star color presets was defined, from which a color was randomly selected during generation (see #ref(<ExampleStarColors>)).

#figure(
  grid(
      columns: 3,     // 2 means 2 auto-sized columns
      gutter: 2mm,    // space between columns
      image("images/System/RedStar.png"),
      image("images/System/BlueStar.png"),
      image("images/System/OrangeStar.png")
  ),
  caption: "Example solar system star colors (red, blue, and orange)."
)<ExampleStarColors>

=== Planets
To fulfill the goal of generating solar systems, planets needed to be placed into the systems along with the stars. The planets were initially assigned orbit radii which increased linearly with a base distance from the sun. This proved to be unstable for systems containing moons and with smaller distances between planets. After experimentation, scaling the orbit radii linearithmically proved to provide a better balance between aesthetics and stability. This design choice is inspired by the structure of our own solar system, where outer planets are spaced farther apart than inner ones @planetary-fact-sheet.

In order for a planet to stay at the same distance from its star, it was assigned an initial velocity perpendicular to the vector in the direction of the star. The velocity $v$ scales with the star's mass $m$, the gravitational constant $G$, and inversely with the orbit radius $r$:

$ |v| = sqrt(G dot m/ r) $

Besides the placements of planets, the generator also randomizes the angle around the star it should be placed at, mass, and radius of planets. Unlike reality, in which mass scales with the radius and placing moons, the mass and radius are randomized and independent from each other.

=== Moons <moons-ref>

Procedurally placing the moons followed the same procedure as the planet placement, except using the planet as the central reference point instead of the star. Only the planets from the fourth position outward were allowed to have moons, to avoid gravitational interference due to their closer proximity.

The moons’ orbit radii increased linearly and the spacing between moons was computed by dividing the planet’s orbit increase value by a constant. A higher ratio resulted in moons being closer to their planet and to each other.

The moon's orbital velocity and the initial angle (orbit angle) were calculated in the same way as for planets, except that its planet's mass and position was used instead of the star's. Furthermore, the radius of the moon was randomly chosen based on its planet's radius.

The moon's appearance was created by generating a sphere, followed by randomly positioning craters along its surface. Each vertex was then processed by iterating through all predefined craters to calculate height adjustments based on a method created by Sebastian Lague @SebLagPlanet, using the following formulas:

```cs
  cavity = x * x - 1;
  
  rimX = Min(x - 1 - rimWidth, 0);
  rim = rimSteepness * rimX * rimX;
  
  craterShape = Max(cavity, floorHeight);
  craterShape = Min(craterShape, rim);
  
  craterHeight += craterShape * crater.Radius;
```

An issue that arose when using the _Max_ and _Min_ functions is that only one of the values would be utilized, which could result in the formation of harsh shaped craters (see @MoonSmooth0). To address this, a smooth minimum and maximum was employed. These functions were based on the approach described in Inigo Quilez's article _Smooth minimum for SDFs_ @SmoothMinMax.

The smooth minimum function can be formulated as follows (Inigo Quilez \u{00A9}):
```cs
  SmoothMin(float a, float b, float smoothnessFactor)
  {
    var h = Clamp((b - a + smoothnessFactor) / (2.0 * smoothnessFactor), 0.0, 1.0);
    return a * h + b * (1.0 - h) - k * h * (1.0 - h);
  }
```

The smooth maximum function can be derived by inverting the smoothness factor as follows:

```cs
  SmoothMax(float a, float b, float smoothnessFactor)
  {
    return SmoothMin(a, b, -smoothnessFactor);
  }
```
  
Enabling this smoothing mechanism allowed for better smoothness over craters, as illustrated in @MoonSmooth1.

#figure(
  grid(
      columns: 2,     // 2 means 2 auto-sized columns
      gutter: 2mm,    // space between columns
      image("images/Moon/moon_smooth0.png", width: 70%),
      image("images/Moon/moon_smooth0_wireframe.png", width: 70%),
  ),
  caption: "Craters with smoothness set to 0"
)<MoonSmooth0>
  #figure(
  grid(
      columns: 2,     // 2 means 2 auto-sized columns
      gutter: 2mm,    // space between columns
      image("images/Moon/moon_smooth1.png", width: 70%),
      image("images/Moon/moon_smooth1_wireframe.png", width: 70%),
  ),
  caption: "Craters with smoothness set to 1"
)<MoonSmooth1>
  
To further enhance the visuals of the moons, textures and a normal map were added. However, incorporating textures presented a challenge: the textures became stretched due to vertex manipulations used to create craters (see @MoonTexture0). While vertex positions were adjusted, the UV mapping—which determines how texture coordinates correspond to the 3D model’s surface—was not updated accordingly.

To resolve this issue, triplanar mapping @TriplanarMapping was employed. This technique involves sampling the texture by projecting it from the three basis vectors (x, y, z), effectively "wrapping" the texture around the object. Since triplanar mapping is a built-in feature in Godot, it was simply enabled in the material setting. By applying both a color texture and a normal texture, the moon achieved a rocky surface with well-defined craters as shown in @MoonTexture1.

#align(center,
  grid(
    columns: 2,
    gutter: 2mm,
    [
      #figure(
        image("images/Moon/moontext0.png", width: 70%),
        caption: [Moon with a texture (triplanar disabled)]
      )<MoonTexture0>
    ],
    [
      #figure(
        image("images/Moon/moontext1.png", width: 70%),
        caption: [Moon with a texture (triplanar enabled)],
      )<MoonTexture1>
    ]
  )
)

== Galaxy
In the context of this project, the galaxy @galaxy-term represents the largest scale of the simulation, a vast space populated by procedurally placed solar systems. The following sections introduce the various iterations the galaxy underwent during development.

=== Star Field <star-field-ref>
The first version of the galaxy, a 3D distribution of stars that we called a 'star field' (see @star-field-img). Points were sampled pseudo-randomly with a uniform distribution within a finite cube, and seeding it, to determine where each star would be instantiated. The stars were constructed using a single spherical mesh (see @star-img).

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

=== Disc Galaxy <disc-galaxy-ref>
Thereafter, a version of the galaxy that imitates the formation of a disc galaxy was created. A disc galaxy is characterized by a flat, rotating disc structure, with a greater concentration of stars at the center @disc-galaxy.

This was achieved by sampling pseudo-randomly with a uniform distribution, within a sphere instead of a cube. The distribution was also influenced by reducing the probability of a star being placed the further away it was located from the galaxy center, thus:
 - reducing vertical spread, which would mimic the flattened shape of a disc.
 - increasing the probability of stars being placed near the center, resulting in a greater concentration of stars near the center.

This resulted in a galaxy with a disc-like distribution, as shown in @disc-galaxy-img. 

#figure(
  image("images/Galaxy/disc_galaxy.PNG", width: 60%),
  caption: [Disc galaxy],
) <disc-galaxy-img>

=== Skybox <skybox-ref>
A traditional skybox was created in Blender @blender @blender-youtube to serve as a pre-rendered galaxy background. Unlike the procedurally generated star fields, it does not contain actual 3D stars, but instead imitates a dense star field using a single static image, as seen in @skybox-testing-img.

#figure(
  image("images/Galaxy/skybox_testing_environment.PNG", width: 60%),
  caption: [Skybox testing environment],
) <skybox-testing-img>

This approach was mainly used for presentation and testing purposes. Since the final goal was to use a backdrop composed of actual, explorable stars, this implementation was not intended for the final product.

=== Infinite Galaxy <infinite-galaxy-ref>
This version is based on the original star field concept from @star-field-ref, this time, expanding infinitely in all directions rather than being limited to a confined structure (see @infinite-galaxy-img).

Additionally, star placement was further refined by sampling from a noise texture. This approach was used to influence clustering, creating areas of higher and lower star densities, to make the galaxy more varied and visually interesting.

#figure(
  image("images/Galaxy/infinite_galaxy.PNG", width: 70%),
  caption: [Infinite galaxy],
) <infinite-galaxy-img>

To support infinite exploration, the galaxy space was divided into discrete chunks (see @B-chunks). Only chunks in the player's closest vicinity are generated and rendered, while distant chunks are culled to save performance. As the player moves, new chunks are generated procedurally, giving the illusion of an endless galaxy. See an example "Star chunk" in @star-chunk-img.

#figure(
  image("images/Galaxy/star_chunk.PNG", width: 70%),
  caption: [Star chunk],
) <star-chunk-img>

=== Finite physics-based galaxy <physics-galaxy-ref>
An Infinite galaxy is a compelling concept, but applying physics to stars of an ever-expanding galaxy is not doable. Since such galaxies are infinitely vast, there is not any fixed point of reference making any attempt at global physics calculations not work.

With great advancements in the physics engine (@physics-engine-ref), an attempt to simulate physics of a finite disc-shaped galaxy was performed, with the physics no longer only confined to the bounds of the solar system.

To test this idea the disc galaxy implementation (@disc-galaxy-ref) was revisited and repurposed. It was retrofitted with new stars containing mass and velocity, to interact with each other through the physics engine. The resulting galaxy is demonstrated below in @phys-galaxy-before and @phys-galaxy-after:

#align(center,
  grid(
    columns: 2,
    gutter: 10pt,
    grid.cell([
      #figure(
        image("images/Galaxy/physics_galaxy_before.PNG", width: 220pt, height: 160pt),
        caption: [Physics galaxy - Before],
      )<phys-galaxy-before>
    ]),
    grid.cell([
      #figure(
        image("images/Galaxy/physics_galaxy_after.PNG", width: 220pt, height: 160pt),
        caption: [Physics galaxy - After]
      )<phys-galaxy-after>
    ])
  )
)

All masses were set equal and no initial velocity was set, resulting in an unstable galaxy. However, it still demonstrated the potential of simulating a galaxy using the physics engine. With 10,000 stars in this initial setup, the performance impact remained minimal. Given more time, setting appropriate masses and velocities would have been explored further.

== Galaxy Map <galaxy-map-ref>
The Galaxy Map unified the project's various components, connecting the procedurally generated infinite galaxy (@infinite-galaxy-ref) with explorable solar systems (@system-gen-ref) and their planets, offering a cohesive experience.

=== Selectable Stars <selectable-star-ref>
To enable interaction with individual stars, the selectable star was implemented. Players could now hover over a star with the mouse cursor and click to select it. This was achieved by adding a spherical collision shape to the star object, which detects mouse input events within the collider. See the star, and its collider in @selectable-star-img.

#figure(
  image("images/Galaxy/selectable_star.PNG", width: 55%),
  caption: [Selectable star],
) <selectable-star-img>

To indicate that a star has been selected, the star's location in space is highlighted, together with a distance measured in "Astronomical Units" (AU) @astronimical-unit. This is visible in the center of @galaxy-map-img. In addition, the coordinates and unique seed of the star are displayed in the bottom-right corner.

=== Navigation <galaxy-map-navigation-ref>
Two modes of transportation were implemented for navigating the Galaxy Map.

1. Manual movement (see @player-controls-ref).
2. Fast travel: Once a star is selected, press the "->"-button in the bottom-right of @galaxy-map-img. This moves the player rapidly towards it, stopping a short distance away.

To explore the solar systems themselves, the "Explore"-button in the bottom-right of @galaxy-map-img, can be used to enter the solar system currently selected. When pressed, a solar system is generated based on the selected star's seed and transitions the player into it. This system exists in a separate scene from the Galaxy Map.

#figure(
  image("galaxy-map-update.png", width: 80%),
  caption: [Galaxy map],
) <galaxy-map-img>

=== Seed Creation <seed-ref>
The galaxy utilizes a set seed, the same used in @infinite-galaxy-ref, to deterministically generate the placement of stars. With the implementation of explorable solar systems, a need arose to generate new seeds for each system. Were they to utilize the same seed, all solar systems would be identical.

To address this, a custom hash function was developed @hash-function, allowing for the generation of unique and deterministic seeds for each star. This function takes into account both the initial Galaxy Seed, and the X, Y, and Z coordinates of a star's position, to produce a star-specific seed. This new seed is then propagated into the stars generation algorithm, which results in unique solar systems while still ensuring deterministic generation.

=== Multi-Meshed Stars & Star Finder <star-multimesh-and-finder-ref>
As the galaxy expanded, loading new star chunks (@star-chunk-img) caused significant stuttering. This performance issue was attributed to the high overhead of instancing hundreds of individual stars, each comprising a `MeshInstance3D` @godot-meshinstance3d and a collider (@selectable-star-ref).

To address this, star rendering was refactored to use Godot's `MultiMeshInstance3D` @godot-multimeshinstance3d. This drastically reduced the number of scene nodes and GPU draw calls (from one per star to one per chunk). As shown in @multi-mesh-performance-table, this optimization substantially mitigated frame time spikes (1% and 0.1% highs) and decreased average memory usage from 115.9 MB to 81.7 MB, enhancing performance stability.

#figure(
  table(
  columns: (auto, auto, auto, auto),
  inset: 7pt,
  align: left,
  table.header([],[*Average*], [*1% high*], [*0.1% high*]),
  [From], [0.39 ms], [3.88 ms], [33.43 ms],
  [To], [0.44 ms], [1.04 ms], [1.9 ms],
  ),
  caption: [Frame time metrics on PC-2 (@pc-2-specs) - Multi-mesh stars],
) <multi-mesh-performance-table>

However, `MultiMeshInstance3D` only instances visual meshes, rendering the collider-based star selection unusable. Two solutions were considered: instantiating separate colliders, or a ray-based approach. We implemented the latter, termed "Star Finder" (see @star-finder-img). When the player clicks, it casts a ray and checks for nearby known star positions at intervals, allowing selection without individual collider objects. This avoided potentially reintroducing the node instancing overhead that the `MultiMeshInstance3D` was intended to solve. The Star Finder features adjustable parameters, like increasing check radius with distance for easier selection of remote stars.

#figure(
  image("star-finder-updated.png", width: 80%),
  caption: [Star Finder tool],
) <star-finder-img>

=== Seamless Galaxy
With great improvements in optimizing planet generation, as detailed in @planet-optimize-ref, new opportunities emerged. Previously, transitioning from the galaxy scale into individual solar systems was a static process, triggered by a button click (@galaxy-map-navigation-ref), which loaded a separate system scene. But now, system scenes could be dynamically instantiated in real-time as the player moves towards a star. This allows the galaxy to be populated by fully realized solar systems that load seamlessly during exploration. An example of multiple systems visible at the same time is shown in @seamless-galaxy-img.

#figure(
  image("images/Galaxy/seamless_galaxy_multiple_systems.png", width: 78%),
  caption: [Seamless galaxy showing two solar systems at once],
) <seamless-galaxy-img>

Using the Star Finder (@star-multimesh-and-finder-ref), stars in a radius around the player could be continuously detected and instantiated at a set distance. As the player approached, these solar systems would scale up slightly until they reached their full size.

However, this approach had notable performance consequences, even with the planet generation optimizations (detailed in @seamless-galaxy-table). Although the average frame time remained stable, some runtime stutters occurred, indicated by the 1% and 0.1% high frame times.

#figure(
  table(
  columns: (auto, auto, auto, auto),
  inset: 7pt,
  align: left,
  table.header([],[*Average*], [*1% high*], [*0.1% high*]),
  [From], [4.17 ms], [5.16 ms], [11.86 ms],
  [To], [4.21 ms], [9.05 ms], [34.31 ms],
  ),
  caption: [Frame time metrics on PC-2 (@pc-2-specs) - Seamless galaxy],
) <seamless-galaxy-table>

In addition, the UI received updates (seen in @seamless-systems-ui-img) to display more information about each solar system.Together with some added flair of an assigned stellar classification @britannica-stellar-classification roughly associated to its color, as well as a randomly selected star catalogue acronym, followed by an integer number @star-naming (the system's seed).

#figure(
  image("images/Galaxy/seamless_galaxy_ui_update.png", width: 80%),
  caption: [Updated Star Select UI],
) <seamless-systems-ui-img>

= Result
The following section presents the final result of the project. First, a brief overview of the final product is provided in @result-overview. Then, in subsequent sections, a more technical in-depth look at the final product will be presented.

== Overview <result-overview>
The final product includes a galaxy map where users can freely navigate between stars, where each star represents the central body of its own solar system. The galaxy map and all systems are generated deterministically, meaning that each time a user enters the same system, it will be generated in exactly the same manner as the first time. 

Entering a solar system causes its belonging celestial bodies (planets and moons) to be generated. The celestial bodies are generated procedurally using 3D-noise as well as the Marching Cubes algorithm, which enables complex terrain generation with overhangs. The moons have craters and a "moon-like" appearance to them and planets are colored based on their distance from the central sun; the closest planets have a "warmer" color palette while the further way planets have a "cooler" color palette. Furthermore, the planets have atmospheres, as well as oceans and vegetation procedurally generated on their surfaces.

Finally, the end product includes a robust physics engine that is capable of updating thousands of objects simultaneously in real-time.

== Planets
The final planets included mesh generation using the Marching Cubes algorithm as well as planetary atmospheres, diverse color schemes, vegetation and oceans. The finished planets are shown in @fig:planet-with-grass-trees and @fig:ocean-planet;

=== Mesh Generation
The planets' meshes were generated using the Marching Cubes algorithm (@marching-cubes), using a scalar field generated using noise (@background-noise-ref) and fBm (@fbm-planets) as input.

=== Atmospheres
The planetary atmospheres were based on ray marching and Rayleigh scattering, which allowed for simulating realistic atmospheric light scattering. Furthermore, multiple color presets were created to ensure visually pleasing results (see @AtmosphereColors for results).

=== Color Schemes
The planets were given color schemes based on their distance from the sun, representing how warm they were. The color schemes were given variation by coloring the fragments based on distance to the lowest point on the planet Cliff faces were also colored. See @fig:cliff-face and @PlanetColors for results.

=== Surface Details and Features
Surface details (@p-surface-details), such as grass, were generated by extracting the planets mesh data and calculating the number of grass blades per face based on the ratio between the total mesh area and the area of the current face. Subsequently, the surface detail was oriented according to the face's normal vector.

For surface features (@p-surface-features) such as trees, generation was achieved by selecting random points within the planet's axis-aligned bounding box (AABB) and performing raycasting. The orientation of the surface feature was then determined based on the result of the ray hit.

#figure(
  image("images/Planet/planetwithgrasstrees.png", width: 65%),
  caption: [Close-up of a planet with trees and grass]
)<fig:planet-with-grass-trees>

=== Oceans
The oceans were generated by applying a water shader to a sphere. The water shader displaced the vertices using noise textures along with normal maps to simulate waves. Additionally, the water color was based on depth extracted from the depth texture. Furthermore, transparency was achieved by utilizing the screen texture. Finally, a foam effect was implemented by blending the water color with a white color at the shallow parts of the ocean.

#figure(
  image("images/Ocean/water_planet1.png", width: 160pt, height: 160pt),
  caption: [Planet with ocean],
)<fig:ocean-planet>

== Systems and Orbits
Systems are deterministically generated from a seed, containing a single star and 3 to 8 planets, with moons placed around the outer planets. While systems are initially stable, numerical errors in the simulation accumulate over time, leading to eventual instability. This instability is more likely when celestial bodies move at higher velocities, such as when the gravitational constant _G_ is set to a higher value.

== Physics Engine <physics-engine-result-ref>
The Rust-based N-body physics engine (@physics-engine-ref) was successfully developed, incorporating parallelized versions of both Direct Summation and a Barnes-Hut algorithm. Benchmarks (@physics-benchmarking-ref) validated its design and optimization. As illustrated in @fig:calc-acc-bench, the parallel Barnes-Hut method demonstrated its capability to calculate accelerations for tens of thousands of bodies (up to approximately 45,000) within a 16.7 ms frame budget, suitable for real-time simulation of large systems at 60 FPS.

In practice, the engine primarily performs small scale calculations to govern solar system dynamics. For this scale, it defaults to the Direct Summation method, due to its superior performance for small $N$ (see @physics-benchmarking-ref). This means that the advanced Barnes-Hut optimization for large $N$ was not utilized in the main gameplay loop.  While this underutilizes the engine’s large-scale capabilities, the choice supports stable and efficient local orbital mechanics in line with gameplay priorities, which emphasize galaxy-scale exploration over high-resolution interstellar dynamics.

The engine's large-scale capabilities were demonstrated in the "Finite physics-based galaxy" (@physics-galaxy-ref), which simulated 10,000 interacting stars in real-time. Although currently used for small-scale systems, the engine is well-suited for future expansion to more complex simulations with a greater amount of bodies.

== Galaxy <result-galaxy-ref>
The galaxy is explorable by the player through movement via the player controls, as well as through selecting a star and fast-traveling to it, with an action under the 'Actions' tab (@galaxy_map_result_1). Solar systems are instantiated and appear as the player approaches them.

With The Galaxy Map implementation (@galaxy-map-ref), the distribution of stars from the Infinite Galaxy connects seamlessly with the Solar Systems implementation, which in turn connects to the planets. Each step in scale is demonstrated in the following figures, the galaxy-scale (@galaxy_map_result_1), towards the system-scale (@galaxy_map_result_2), eventually reaching the planet-scale (@galaxy_map_result_3).

#figure(
  image("images/Galaxy/Result/galaxy_map_result_1.png", width: 85%),
  caption: [Galaxy Map at the galaxy-scale],
) <galaxy_map_result_1>

#figure(
  image("images/Galaxy/Result/galaxy_map_result_2.png", width: 85%),
  caption: [Galaxy Map at the system-scale],
) <galaxy_map_result_2>

#figure(
  image("images/Galaxy/Result/galaxy_map_result_3.png", width: 85%),
  caption: [Galaxy Map at the planet-scale],
) <galaxy_map_result_3>

The seamless instantiation of systems was left as a toggleable option. By enabling it, you get the result that is seen in the images above. However, the resulting performance implications on the dedicated benchmarking computer (PC-1, Specs: @pc-1-specs) is significant, see @galaxy-map-seamless-result-table:

#figure(
  table(
  columns: (auto, auto, auto, auto),
  inset: 7pt,
  align: left,
  table.header([],[*Average*], [*1% high*], [*0.1% high*]),
  [Seamless Galaxy disabled], [0.88 ms], [1.67 ms], [4.51 ms],
  [Seamless Galaxy enabled], [1 ms], [5.35 ms], [17.08 ms],
  ),
  caption: [Frame time metrics on PC-1 (@pc-1-specs) - Seamless galaxy],
) <galaxy-map-seamless-result-table>

Compared to our performance goals, as dictated earlier in @benchmarking-and-performance-ref, looking at @galaxy-map-seamless-result-table with the seamless galaxy enabled, the average FPS is $1003.8$ and the greatest frame time disparity is $16.8$ ms. This, achieves one out of two of set goals.

By disabling seamless instantiation, the average FPS is $1135.6$ and the greatest frame time disparity is $3.63$ ms. This, falls within our performance goals.

= Discussion
This section presents a discussion and reflection on the results, process, and methodology of the project. It also addresses the generalizability and validity of the findings. Furthermore, societal and ethical considerations are discussed. The section concludes with an exploration of potential directions for future work within the project.

== Result Discussion
The following subsection discusses the project's results, discussing the role of the MoSCoW table as a tool in achieving these outcomes. It also addresses the considerations involved in balancing performance with the trade-offs between realism and gameplay. Finally, a comparison is made with Exo Explorer to discuss the differences.

=== The MoSCoW Table <ResultDisc-MoSCow>
As shown in @MosCowFinished, the majority of the project tasks were successfully completed. This outcome may be attributed to a well-structured planning phase and effective scope management. Additionally, the inherent vagueness of certain features may have contributed, as their ambiguous nature allowed them to be interpreted either as substantial or minor tasks.

Using a MoSCoW table helped us think carefully about the priority of each feature, making sure the project stayed focused on its main purpose. This meant that even if we didn’t manage to finish all of the _Should Have _or _Could Have_ items, the core of the project would still be complete and aligned with its purpose. For example, the vegetation feature, listed as a _Could Have_, was something we thought would add visual interest, but it wasn’t essential to the project’s objectives. Keeping these priorities in mind helped the team stay focused on what mattered most and avoid spending time on features that didn’t contribute meaningfully.

However, the application of MoSCoW was not without shortcomings. Some features were defined imprecisely, leading to occasional overlap and ambiguity. For example, the design of the navigation interface lacked clarity and appeared to intersect conceptually with the galaxy traversal system, resulting in confusion regarding their integration and implementation.

A notable challenge was the project's open-ended nature, which, while fostering creativity, impeded the formulation of concrete and well-defined concepts. For instance, the feature "_get interesting info about planets and solar systems_" lacked specificity, as the term "_interesting_" was not clearly defined beyond a few examples. Nevertheless, this issue was largely mitigated during the task breakdown phase for the Kanban board, which provided the necessary clarity to proceed effectively.

=== Balancing performance, visuals and user experience
While visuals were a consideration in this project, the main focus was to ensure that the program ran smoothly and performed well. In some cases, such as deciding how detailed the planets should be, we had to balance performance and visuals. Ultimately, visual fidelity was kept within reasonable limits, as long as it didn’t compromise our set performance goals.

Initially, the resolution of the planets directly correlated with the amount of data points within them, meaning that a planet with a larger radius required more iterations to generate. This posed a problem when scaling up the galaxy as the planets became too demanding to generate. Therefore, by separating the radius from the amount of data points (the resolution of the planet) it became possible to scale the planets without increasing their resolution. When creating a planet with low resolution, the performance of generating planets became better, at the expense of the visual fidelity of the planet.

To reduce stuttering when loading in a new solar system (due to the planets generating), the planet generation was offloaded to different threads (@worker-thread-pooling-ref). To avoid planets popping in during gameplay, they were given a temporary mesh that got replaced once their real mesh had been constructed. This was an example of where the visuals were directly impacted by the performance, albeit temporarily during gameplay. It also affected the user experience due to players having to wait for the planets to be constructed. We felt that this was a good compromise between user experience and visuals because otherwise players might get frustrated when the game freezes each time a solar system loads.

=== Balance Between Realism and Gameplay <BalanceRealismGameplay>
Balancing realism and gameplay was a recurring challenge. The solar systems are not to scale, as realistic distances made planets too far apart to be visible during exploration. To address this, planets where moved closer to each other. As talked about in @system-gen-ref, this increased risks of instability.

Planetary motion also posed challenges for surface exploration, as moving planets affected player physics. One considered solution was to freeze a planet’s movement when a player was on it, simplifying implementation by removing velocity effects. However, this was rejected in favor of maintaining physical accuracy, therefore, planets continue moving at all times.

=== Exo Explorer Differences
This project explored similar areas as the previously mentioned _Exo Explorer_, but with a greater emphasis on an advanced physics engine, proper benchmarking, and real-time exploration of a procedurally generated galaxy. By contrast, Exo Explorer focused on a single solar system, allowing for deeper detail in planetary environments, whereas this thesis prioritizes scalable procedural systems suitable for rendering and simulating large-scale space exploration.

A key difference between the two projects lies in their approach to realism. As briefly discussed in @BalanceRealismGameplay, one proposed solution to address issues related to planetary landings involved freezing a planet’s motion when a player is present on its surface and transitioning to a geocentric reference frame. This method was inspired by _Exo Explorer_, which adopts a similar approach.

Finally, the most significant distinction is the implementation and optimization of a large-scale physics model for simulating celestial dynamics, which was an element that was not developed in _Exo Explorer_.

== Process and Method Discussion
This subsection reflects on the process and methods used, including workflow choices, use of multiple programming languages and AI, and changes in the original plan.

=== Multiple Programming Languages
This project utilized a multi-language approach, via Godot's GDExtension system @GDExtension (detailed in @P-Godot). We utilized a mixture of GDScript, C\# and Rust, allowing us to select the optimal language for specific tasks.

For computationally intensive components, the physics engine (@physics-engine-ref) in particular, Rust was chosen. Its strengths in raw performance, memory safety, and concurrency enabled significant optimization @klabnik2023rust.

However, it also introduced complexities. Managing a multi-language code base, debugging across languages, and passing data between the languages required setup and proved to be cumbersome on occasions.

Overall the experience was positive, leveraging each language's strengths was advantageous for achieving the project's simulation goals despite the added overhead.

=== Workflow and Collaboration
The group followed an Agile-inspired workflow (@Workflow) utilizing a GitHub Projects Kanban board for task management, Git for version control with a feature-branch model (@Git-section), and Discord for team communication. GitHub also facilitated pull request (PR) reviews and issue tracking.

This structured approach was generally effective, with tasks discussed, prioritized, and assigned during weekly team meetings. However, sometimes PRs would lay waiting for a review on GitHub a longer time, since we had not established any process to assign reviews, and it was up to the initiative of each member. This could lead to PRs getting outdated, creating problematic merge conflicts and requiring more time to bring branches up to-date. Implementing a stricter system of handling these reviews would therefore have been beneficial.

Despite these challenges, core elements such as feature branches, mandatory PRs, and centralized task tracking were crucial for effective collaborative development throughout the project.

=== Use of Generative AI
Artificial intelligence (AI) was utilized at various stages throughout the project. During the development phase, tools such as ChatGPT @ChatGPT and GitHub Copilot @Copilot were employed to support the coding process. Copilot was also integrated into the PR review workflow, providing quick feedback on code submissions. While AI-generated reviews were not considered substitutes for peer-reviewed evaluations, they offered an efficient means of identifying obvious issues (e.g. noticing when a wrong variable was used) that might otherwise be overlooked.

Additionally, AI was employed during the report writing phase. Tools such as ChatGPT and Google Gemini @Gemini were occasionally used to refine already written text and improve the overall quality of the writing, by for example, finding grammatical errors and spelling mistakes 

=== Project Purpose
The project's purpose underwent a greater change after early feedback on the initial planning. The original purpose was as follows:

#quote(block: true, quotes: true)[
  The aim of this project is to simulate solar systems through procedurally generated planets, utilizing computer-generated noise such as Perlin noise, together with the Marching Cubes algorithm. The composition of the solar systems can vary – from a sun with a single planet to more complex systems with multiple planets and additional celestial bodies such as moons. To mimic the natural movements of these celestial bodies, a simplified physics simulation will be implemented.
]

This project also aims to explore and combine different techniques for optimization to ensure that the simulation will run in a performance-efficient manner."

It became clear that the project’s objectives were not clearly defined. After internal discussions the team reached a consensus on the purpose of the project. Any changes, particularly to the purpose, sought to address the following three problems:

1. A great deal had been explained specifically about solar systems in the planning, while the team, in reality, had drifted towards wanting to create an entire galaxy of solar systems instead.
2. An entire section of the planning was dedicated to performance and optimization, as well as a part of the purpose. This played a part in making it unclear whether this project's major focus was about optimization, or something else.
3. It was unclear how this project differs from the similar bachelor's thesis project Exo Explorer @exo_exporer:2023.

Through internal discussions, consultation with our supervisor, and study of the aforementioned feedback, the purpose was rewritten to refine the project's goals and scope (see the rewritten purpose in @purpose-ref).

This helped align the team as a whole. Putting the focus on implementing a galaxy of solar systems, rather than a single one. As well as reducing the focus on performance and optimization techniques, instead focusing on finding techniques to reach an at least viable performance.

=== MoSCoW Changes
The MoSCoW method was used to structure and prioritize project features into tiers of importance. As the project progressed and its scope became clearer, the MoSCoW table underwent change. Due to the agile-inspired workflow of the project, features were re-prioritized, removed or added.

While most features remained unchanged throughout, some of the most notable changes were:

- Camera/player controls: Moved from "Should have" to "Must have" as the focus on being able to explore the planets, systems, and galaxy, was deemed very important.
- Space background: Removed from "Must have", since it was deemed not critical for project's success. Although, a space background was eventually given by itself as the stars were distributed in the galaxy.
- Galaxy traversal: Added as a "Should have" to allow for traversal at different scales. On the solar galaxy level, the solar system level, and on the planet level.
- Other celestial bodies e.g. asteroids or nebulae, as well as planet vegetation, were added as potential features to be added. In the end planet vegetation was explored.

=== Performance Methodology
At first, in the initial planning, the key performance metrics to evaluate the application were:

- *Memory consumption:* Evaluating the amount of system memory utilized during execution.
- *Scene generation time:* Measuring the time it takes to generate a scene.
- *Frames per second (FPS):* Assessing the rendering performance of the simulation.

Shortly thereafter, a target of maintaining an average of 60 FPS was determined as a concrete benchmark to strive toward. This target was to be achieved on a specific benchmarking computer with specific hardware specifications, those specifications were detailed in *PC-1* (see @pc-1-specs).

However, as development progressed and with greater research into real-time performance, the team improved its understanding of what constitutes a smooth and responsive performance. This, with the focus shifting towards consistency in frame times instead, this updated benchmarking methodology is described in @benchmarking-and-performance-ref.

Regarding the other original metrics:

- *Scene generation time:* This refers to how long it takes to initialize and load new scenes and was initially marked as a performance metric. However, since the majority of elements are streamed at runtime, rather than through traditional loading screens in between, the metric was eventually deemed less critical. Even so, it was still utilized as a metric for some operations, such as during planet generation, to compare different implementations and optimizations.

- *Memory consumption:* While initially a concern, it proved not to be a limiting factor in practice. This is likely due to the content being generated procedurally at runtime, rather than pre-loaded or stored in memory. Although it was continuously monitored, no memory-related issues occurred, making it a non-critical performance metric for this project.

== Generalizability and Validity
This section considers the broader applicability of the project's components and the soundness of its simulation results.

=== Generalizability 
Many techniques employed in this project are generalizable. The implemented N-body algorithms are standard methods applicable to other systems governed by inverse-square laws (like gravity or electrostatics). The Morton-based octree construction could be used for additional particle simulations. Optimization strategies such as octree-driven LOD, chunking, and CPU parallelism (e.g., with rayon) are common techniques, as well as procedural generation methods with noise and mesh generation with Marching Cubes.

=== Validity <validity>
Evaluating the validity of this project requires considering the goal of achieving the most scientifically accurate physics model possible within the constraints of a smooth, real-time simulation.

1. *Physics Simulation*: The simulation is grounded in established physical principles, using Newton's Law of Universal Gravitation and standard N-body algorithms like Direct Summation and Barnes-Hut. However, achieving the necessary performance led to certain trade-offs regarding accuracy:

  - _Integration Method_: We implemented the symplectic Euler method for time integration. While selected for its improved stability over explicit Euler (crucial for preventing orbits from rapidly degrading in real-time), it is a first-order integrator. This means it is more prone to numerical errors and energy conservation errors over long simulations compared to higher-order symplectic methods (e.g. Leapfrog or higher-order Runge-Kutta @numerical_recipes), which are often used in non-real-time scientific studies. This choice represents a direct trade-off between accuracy and the computational budget per frame needed for smoothness.
    
  - _Omitted Physics_: To maintain real-time feasibility, complex physical interactions beyond basic Newtonian gravity were omitted. This includes relativistic effects, physical collisions between bodies and non-gravitational forces.
  
  - _Outcome_: The resulting simulation produces visually plausible orbital mechanics suitable for an interactive exploration context, but its accuracy for scientific purposes is limited by these simplifications.

2. *Procedural Generation:* The validity here pertains to internal consistency and alignment with aesthetic goals. The use of deterministic, seeded generation ensures that the galaxy and its systems are consistently reproducible. While using physically inspired concepts (noise, fBm), the procedural generation prioritizes visual diversity and exploration potential over astrophysical realism.

In essence, the project's validity lies in its successful implementation of recognized N-body algorithms and optimization techniques to create a large-scale, real-time simulation that aims for physical realism while accepting necessary compromises to ensure interactivity and performance targets were met.


== Societal and Ethical Aspects
Two main points of discussion were identified: how procedural content generation (PCG) affects game designers, particularly level designers, as well as its impact on players.

As PCG and artificial intelligence (AI) advances, there's a risk that it could replace human developers. While PCG reduces cost and development time @computers13110304, the concerns that it may become proficient enough to replace human creativity are still present. An example for this is when the Swedish game company Mindark announced plans to fire half of their employees, primarily world builders, in favor of AI-driven content generation @MindarkAftonbladet.

However, the development process in this project revealed that significant manual effort remained necessary. Generating aesthetically pleasing content and scaling and positioning the celestial bodies in a plausible manner all required substantial manual tweaking of parameters and settings. Moreover, PCG algorithms have the potential to generate content that may be faulty, unrealistic or unnatural. Solutions to these issues include setting specific constraints before executing the algorithms, or involving a human designer at the end of the content generation #cite(<computers13110304>). Although it is still a possibility that future PCG algorithms can automate these processes, current implementations seem to depend on a collaborative relationship between human designers and algorithmic systems. 

Another concern was that PCG-generated content must be sufficiently engaging and playable to avoid negatively impacting the player experience. Games containing PCG are at risk of containing repetitive content, which may influence a player's sense of immersion or reduce replayability. An example is No Man’s Sky, where the procedurally generated planets felt overly repetitive and basic @pcgchallanges:2017. Additionally, PCG can also create environments that hinder gameplay, such as untraversable terrain, negatively affecting the overall experience.

Although the primary objective of this project was not to create an engaging game play experience, certain measures were nonetheless taken to reduce repetitiveness. For instance, planetary coloration was randomized based on each planet’s distance from the sun, with additional randomization applied to simulate variations in atmospheric thickness. These methods introduced greater diversity in the generated content, demonstrating that careful parameterization and randomness can effectively counteract some of the inherent risks associated with procedural generation.

== Future work
There are several directions in which this project could be expanded. A number of planned features were not implemented due to time constraints, and these could serve as valuable additions in future iterations.

In particular, the planet generation system offers considerable room for improvement. Currently, generated planets feature basic elements like vegetation (e.g., trees and grass) and bodies of water (e.g., oceans), but they remain relatively unengaging. Future enhancements could include the addition of fauna, biome-specific details, and subterranean structures such as cave systems, this would also make greater use of the existing Marching Cubes implementation, which is well-suited to handle mesh generation of caves and overhangs.

Additionally, as identified in @Octree-Planet, where gaps appear between differing LOD chunks. This issue can be solved in future iterations through the implementation of the Transvoxel algorithm @Transvoxel2010, which is designed to solve this issue. 

Another potential extension involves incorporating a wider range of celestial bodies, such as gas giants, meteoroids, and comets, which would significantly enhance the diversity of the galaxy. Multiple of these, in particular the solid-surface bodies, could make use of the same, but modified, planet generation system. Additional celestial bodies was listed as a "Could have" in the MoSCoW prioritization but could not be implemented within the project timeframe.

Another opportunity for future development is making further use of the existing physics engine (see @physics-engine-ref). The engine's large-scale capabilities were demonstrated with the Finite physics-based galaxy (@physics-galaxy-ref), as well as studied in @physics-benchmarking-ref. Initially, work would continue on the physics-based galaxy to make it stable.

#box[
  = Conclusion
  This project set out to develop and simulate a physics-based, procedurally generated, and explorable galaxy within the Godot engine, addressing the challenges of computational scale, performance, and plausibility in creating such vast virtual environments. The core objective was to produce a deterministic and computationally efficient model capable of real-time interaction.
  
  Development resulted in a comprehensive system. Planets were successfully generated using a combination of noise functions (Perlin, fBm) and the Marching Cubes algorithm, resulting in complex and varied terrains. These planets were further enhanced with procedurally placed features, including oceans with shader-based wave effects, vegetation distributed via Poisson-disc sampling, and physically-inspired atmospheres simulating Rayleigh scattering.
  
  Crucially, significant performance optimizations were implemented and validated; multi-threading effectively offloaded intensive planet generation tasks, while octree-based chunking with Level of Detail (LOD) dynamically managed mesh complexity, and Multi-Mesh instancing drastically reduced draw calls for star rendering. A robust N-body physics engine, leveraging a parallelized Barnes-Hut algorithm implemented in Rust, was integrated to simulate celestial mechanics efficiently for numerous bodies. The use of seeded randomization throughout ensured deterministic generation, allowing for reproducible galaxies and solar systems. Exploration capabilities were successfully implemented, allowing navigation across multiple scales via a galaxy map and seamless transitions into solar systems down to planetary surface interaction with a physics-aware controller.
  
  The project successfully met its primary goals, fulfilling all "Must Have" and "Should Have" requirements defined during planning. The focus on achieving consistent frame times, rather than solely maximizing average FPS, proved effective in delivering a smoother user experience during exploration and simulation.
  
  Ultimately, this work contributes a practical implementation and analysis of techniques essential for simulating large-scale procedural galaxies. It demonstrates the successful integration of advanced procedural generation, N-body physics simulation, and targeted optimization strategies within the Godot engine, offering a viable and efficient model for developers aiming to create expansive, dynamic, and interactive celestial environments. While acknowledging necessary simplifications for real-time performance, the project establishes a solid foundation upon which future enhancements, such as greater celestial diversity or more complex physical interactions, can be built.
]

#[

#pagebreak()
#bibliography("src.bib")

#show: appendix

#pagebreak()

= : Specifications for benchmarking computers <all-pc-specs>

#align(center)[
  #set par(justify: true)
  
  Benchmarking was planned to be performed on a dedicated benchmarking computer named PC-1 (@pc-1-specs), and was used for the final result. However, during @process-ref other machines are occasionally used when PC-1 is not available. These are PC-2 (@pc-2-specs), PC-3 (@pc-3-specs), and PC-4 (@pc-4-specs). 
]

#figure(
  table(
  columns: (auto, auto),
  inset: 7pt,
  align: left,
  table.header([*Component*],[*Specification*]),
  [CPU], [Intel i7-11800H],
  [GPU], [Nvidia GeForce RTX 3050 Ti Mobile],
  [RAM], [64 GB DDR4 3200 MHz],
  ),
  caption: [PC-1 Specifications],
) <pc-1-specs>

#figure(
  table(
  columns: (auto, auto),
  inset: 7pt,
  align: left,
  table.header([*Component*],[*Specification*]),
  [CPU], [AMD Ryzen 7 7800X3D],
  [GPU], [NVIDIA GeForce RTX 4080],
  [RAM], [32GB DDR5 6000MHz],
  ),
  caption: [PC-2 Specifications],
) <pc-2-specs>

#figure(
  table(
  columns: (auto, auto),
  inset: 7pt,
  align: left,
  table.header([*Component*],[*Specification*]),
  [CPU], [Intel Core i7-8850H],
  [GPU], [NVIDIA Quadro P1000],
  [RAM], [32GB DDR4 2267MHz],
  ),
  caption: [PC-3 Specifications],
) <pc-3-specs>

#figure(
  table(
  columns: (auto, auto),
  inset: 7pt,
  align: left,
  table.header([*Component*],[*Specification*]),
  [CPU], [Intel Core i7-13700K],
  [GPU], [NVIDIA GeForce RTX 4080],
  [RAM], [32GB DDR5 5600MHz],
  ),
  caption: [PC-4 Specifications],
) <pc-4-specs>

#pagebreak()

= Pseudocode for the Direct Summation algorithm <pseudo-direct>
#box[
#show figure: set align(left)
#show figure.caption: set align(center)


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

        Assign[$arrow(r)_(i j)$][particles[j].*position* - particles[i].*position*]
        Assign[$m_j$][particles[j].*mass*]
        
        State[]
        Cmt[Note: Softening factor $epsilon$ omitted for brevity, but used in implementation]
        If(cond: [$|arrow(r)_(i j)|> 0$], {
          Assign[$arrow(a)_(i j)$][$G m_j/(|arrow(r)_(i j)|^3) arrow(r)_(i j)$]
          Assign[acc[i]][acc[i] $+ arrow(a)_(i j)$]
        })      
      })
    })

    Return[acc]
  })
})
]

= MoSCoW Table <MosCowFinished>
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
    - #highlight(fill:rgb("#a8ffa4"))[Working terrain-generation using computer generated noise].
    - #highlight(fill:rgb("#a8ffa4"))[There should be at least one procedurally generated planet and sun].
    - #highlight(fill:rgb("#a8ffa4"))[The planet should orbit the stars with as accurately as possible to the laws of physics].
    - #highlight(fill:rgb("#a8ffa4"))[A way to explore the generated terrain using a camera controller].
  ],
  [
    - #highlight(fill:rgb("#a8ffa4"))[To explore different performance techniques to achieve effective procedural generation of planets]
      - #highlight(fill:rgb("#a8ffa4"))[Occlusion, backface, etc, -culling]
      - #highlight(fill:rgb("#a8ffa4"))[Levels of detail (LOD)]
      - #highlight(fill:rgb("#a8ffa4"))[Applied spatial data structures (E.g octrees and grids)].
      - #highlight(fill:rgb("#a8ffa4"))[Chunking]
    - #highlight(fill: rgb("#a8ffa4"))[A technique to draw complex terrain with overhangs and the like, e.g Marching Cubes].
    - #highlight(fill: rgb("#a8ffa4"))[Solar systems can have multiple planets].
    - #highlight(fill: rgb("#a8ffa4"))[Some planets should have moon(s)].
    - #highlight(fill: rgb("#a8ffa4"))[Galaxy. A procedurally generated galaxy constructed out of solar systems]
    - #highlight(fill: rgb("#a8ffa4"))[Galaxy traversal. Ways of navigating between solar systems, using a UI galaxy map or traveling from solar systems in real-time. Zooming out: Solar system level, solar systems in proximity, galaxy level. (examples)]
    - #highlight(fill:rgb("#a8ffa4"))[There should be shaders to improve the look of things in the solar system, such:]
      - #highlight(fill: rgb("#a8ffa4"))[Planet atmospheres]
      - #highlight(fill: rgb("#a8ffa4"))[Water on planets]
    - #highlight(fill: rgb("#a8ffa4"))[Player abides by the laws of physics, interacting with planets by walking for example].
  ],
  [
    - #highlight(fill: rgb("#a8ffa4"))[Get interesting info about planets and solar systems, such as information about a planet's mass, amount of moons]
    - #highlight(fill: rgb("#a8ffa4"))[Planet properties like temperature that change according to, for example, distance from the sun. Hot at equator, cold at poles, etc].
    - #highlight(fill: rgb("#ff8f8f"))[UI for navigating between planets/solar systems/galaxies (fast travel)]
    - #highlight(fill: rgb("#ff8f8f"))[Other celestial bodies. E.g. asteroids, black holes, asteroid belts, gas planets, nebulae, etc].
    - #highlight(fill: rgb("#a8ffa4"))[Vegetation]
  ],
  ),
  caption: flex-caption(
    short: [The project's MoSCoW Table],
    long: [The project's MoSCoW Table showing which tasks were finished (marked green), and which tasks were unfinished (marked red). "Won't have" is elided since the relevant parts are detailed in @I-limitations]
  )
)


]<no-wc>