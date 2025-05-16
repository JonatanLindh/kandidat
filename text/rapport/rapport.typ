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
  [#body <no-wc>]
}

#set heading(numbering: "1.")


#page()[
  #set align(center + horizon)
  #grid(
    rows: (1fr, 10fr, 1fr),
    image("EN_Black_Chalmers_GU.png"),
    [
      #set text(size: 30pt)
      #text(maroon)[Ord: #total-words]
      
      #image("nice image maybe.png", width: 80%)
      Chalmers Galaxy Solar-System Planet Generation Simulation Software System Application
      
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

#show text.where(fill: red): it => [
  #it <no-wc>
]
#show: word-count.with(exclude: <no-wc>)

#pagebreak()
#heading(numbering: none)[
  Glossary
]

- *Mesh:* A collection of vertices, edges, and faces that define the shape of a 3D object.
  - *Vertex:* A point in 3D space, representing a corner of a triangle.  Stores attributes like position (x, y, z), normal, UVs, and optionally color.
    - *Normal:* A vector that is perpendicular to a surface. Used in lighting calculations.
    - *UV coordinates:* a 2D coordinate system used to map a texture image to a 3d object surface.
  - *Edge:* A line segment connecting two vertices.
  - *Face (Triangle):* A flat surface bounded by edges (typically a triangle in real-time graphics). Triangles are used because they are always #text(red)[planar]. #text(maroon)[Staffan: som betyder vad?]
  
- *Rendering Pipeline:* The sequence of stages a #text(red)[GPU] #text(maroon)[S: borde vara med i glossary] uses to render a 3D scene.

- *Shader:* A small program that runs on the GPU. Primarily used for controlling how objects are rendered.
    - *Vertex Shader:* Processes each vertex; transforms positions from model space to clip space and manipulates vertex attributes.
      - *Model Space:* The object's local coordinate system.
      - *Clip Space:* A standardized coordinate system used by the GPU after vertex transformation.
    - *Fragment Shader (Pixel Shader):* Determines the final color of each fragment, often using lighting and textures.
      - *Fragment:* A "potential pixel" generated during rasterization.
      - *Rasterization:* The process of converting vector graphics (triangles) into raster graphics (pixels).
    - *Compute Shader:* Used for general-purpose computation on the GPU (not directly part of the rendering pipeline).

- *Procedural Generation:* The creation of data (models, textures, etc.) algorithmically, rather than manually. This often uses noise functions and other mathematical techniques to create varied and complex results.
  - *Seed:* A value used to initialize a pseudo-random number generator. Using the same seed ensures the same sequence of "random" numbers @godot-random-number-generator.
  - *Noise Function #text(red)[(Perlin, Simplex)] #text(maroon)[S: ta bort?]:* Algorithms for generating pseudo-random values with a smooth, continuous appearance, used for procedural generation.
  - *Marching Cubes:* An algorithm to create a triangle mesh from a 3D scalar field (e.g., noise data).

- *LOD (Level of Detail):* Rendering objects with varying complexity based on distance.
- *PCG*: Procedural content generation
- *Axis Aligned Bounding Box (AABB):*  A box where each face is aligned to the coordinate vectors (x, y, z).

- *Performance metrics:*
    - *FPS (Frames per second):* The amount of images (frames) that a computers render every second @nvidia_fps. Higher FPS generally means smoother motion. For example, 60 FPS means the screen is updated 60 times per second. Closely related to frame time.
    - *Frame time:* The amount of time (in milliseconds) it takes to render a single frame @techreport_inside_the_second. Closely related to FPS, e.g., 60 FPS = \~16.67 ms frame time per frame.

#set page(numbering: "1")
#counter(page).update(1)

= Introduction #text(red)[ERIK KLAR]
Procedural content generation (PCG) offers a way to algorithmically create vast and diverse game worlds without the immense manual effort required to design every detail. Applications of PCG range from the random placement of enemies in confined dungeon spaces to the generation of entire universes comprising millions of celestial bodies. Using PCG also has the potential to increase re-playability #cite(<PCGNeuroevolution>).

Using PCG algorithms is particularly relevant in the context of space exploration games, where the scale of the universe is inherently beyond manual creation. Creating compelling, varied, and believable planetary systems and galaxies is a challenging problem within this domain. Key issues include the computational efficiency required to generate hundreds to millions of celestial objects, as well as the need to balance performance constraints with the goal of providing a plausible and playable experience. 

This thesis addresses these challenges by developing a system for procedurally generating a galaxy composed of multiple solar systems, with an emphasis on computational efficiency and physical accuracy. The implementation is built in the Godot game engine, and requires utilization of techniques in this domain, such as utilizing the power of GPU computations, different terrain generation algorithms, and physical models. In addition, the project hopes to provide insight into how such challenges can be approached by documenting the thought process behind decisive development decisions.

A related project, Exo Explorer @exo_exporer:2023, a bachelor's from Chalmers University of Technology, explored similar themes. While Exo Explorer focused on a single solar system and emphasized gameplay and planetary ecosystems, this thesis places a greater focus on simulating a physically accurate model of a procedurally generated, explorable galaxy. 

== Purpose #text(red)[Jacob klar] <purpose-ref>
The aim of this project is to create a physics-based simulation of a procedurally generated, explorable galaxy. 

Each solar system that make up the galaxy will consist of various procedurally generated planets, orbiting a central star. These orbits are governed by a simplified physics simulation based on Newtonian physics. System complexity can vary, ranging from a sun with a single planet, to arrangements with multiple planets and moons. #text(maroon)[Staffan: skillnad från tidigare?]

While procedurally generated, the galaxy will remain consistent and revisitable by ensuring deterministic generation. Different seeds will allow for unique galaxies to be created, while also enabling parts to be generated identically, upon revisit.

== Initial Limitations
Some limitations for the projects have been set. The physics simulation will be simplified and not necessarily accurate according to real laws of gravity. This does not mean that the program will completely disregard the accuracy of the physics model, but will instead focus on specific aspects such as the orbit of celestial bodies and how they affect each others orbits. The system needs to be realistic enough to simulate solar systems, but not realistic to a point where all intricacies of physics will be considered.

Furthermore, the project will not be developed into a full scale video game. Rather, the focus will lie on the more technical aspects: terrain generation, physics, and performance optimization. Other features such as a UI for quick travel or detailed planet properties are not prioritized. Essentially, the goal of the project is to create a model of a procedurally generated galaxy, not an engaging game play experience.

== Contribution #text(red)[Erik Klar]
This report hopes to contribute with a computationally efficient, conceptually interesting and accurate physics model for simulating celestial movements in the game engine Godot.

= Background
This section presents the foundational theoretical concepts that were needed before beginning the project, followed by a few select previous works that utilize some of these concepts.

== Procedural Content Generation #text(red)[William klar]
Procedural Content Generation (PCG) is defined as “the algorithmic creation of game content with limited or indirect user input”@shaker2016procedural. While PCG is widely used in video games—typically involving the automatic generation of content such as unique levels for each gameplay session or the stochastic placement of environmental elements like vegetation—it also has applications beyond gaming, such as in architectural design, and data generation.. Throughout the project procedural generation will be used in variation. 

== Noise #text(red)[ANTON typ KLAR] <background-noise-ref>
Noise is very commonly used in a plethora of #text(red)[computer generated content] #text(maroon)[Staffan: Vad är skillnaden på detta om PCG? Om samma, använd samma i hela rapporten (gäller alla begrepp)], such as for generating mountains, textures and vegetation @proceduralgame:2016. Noise is generated through the use of a pseudo random function and is often stored and visualized as a texture. Perlin noise @perlinnoise:1985 is a famous gradient noise function founded by Ken Perlin in 1985 that has been used extensively to produce procedural content in games and films. Noise will be at the core of many of the things developed in this project and will be used frequently.

#text(maroon)[Staffan: Detta förklara hur noise används och skapas men inte vad det är (inom PCG)...]

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

=== Marching Cubes #text(red)[William, Anton]<marching-cubes>
 The Marching Cubes algorithm @marchingcubes:1998 is a method for extracting a polygonal mesh representation of an isosurface from a three-dimensional scalar field. An isosurface is a surface that represents points of a constant value within a volume. A scalar field, on the other hand, is a "_scalar-valued function of the points of a domain in some space, such as the temperature or the density field inside a body_" @scalarfield. The algorithm takes a grid of scalar values as input and generates a mesh approximating the isosurface defined by a given threshold, enabling the visualization of complex structures within the data.

The procedure involves traversing the scalar field and evaluating groups of eight adjacent grid points, which forms a logical cube. For each cube, the algorithm determines the polygon(s) that approximate the isosurface intersecting that region. This is achieved by classifying each vertex of the cube as either inside or outside the isosurface, based on whether its iso-value (the scalar value at that vertex) is above or below the predefined iso-level (the chosen threshold value that defines the isosurface). 
In other words, the iso-level specifies the value that the isosurface represents, while the iso-value is the actual scalar value at a specific vertex. The algorithm then references a precomputed lookup table to identify the appropriate triangulation for the cube's configuration.

Given that each of the eight cube vertices can exist in one of two states (on or off), there are $2^8 = 256$ possible configurations. However, due to symmetry and rotational equivalence, these reduce to 15 unique cases (see #ref(<mc15>)), with all others derivable through reflection or rotation. The process is repeated throughout the entire scalar field, and the resulting polygons are combined to form the final mesh.
#figure(
  image("images/MarchingCube/MarchingCubesEdit.svg"),
  caption: [Marching Cubes 15 unique polygon combination. Ryoshoru, #link("https://creativecommons.org/licenses/by-sa/4.0")[CC BY-SA 4.0], via Wikimedia Commons]
) <mc15>

== Chunks #text(red)[William klar]
Chunks @Chunk1 refers to (in this context) a fixed-size segments of data that is loaded, processed, and rendered independently. This approach is particularly beneficial in large or procedurally generated environments, as it allows the game engine to manage memory and computing resources efficiently by loading only the chunks near the player.

Chunking can be used for additional optimization such as:
- Frustum Culling: Rendering only the chunks within the player's field of view.​
- Occlusion Culling: Skipping the rendering of chunks or objects that are blocked from view by other objects.​
- Level of Detail (LOD): Reducing the complexity of distant chunks to save on processing power.

=== Octrees #text(red)[William klar]<B-octree>
An octree @octree1 is a hierarchical data structure that recursively subdivides three-dimensional space into eight octants using a tree structure (see #ref(<octreeimg>)). This structure is particularly effective for managing sparse or large-scale environments, as it allows for efficient spatial queries, collision detection, and level-of-detail (LOD) rendering.​

#figure(
  image("images/Octree/Octree2.svg.png", width: 65%),
  caption: [Visualization of a in both a cube- and tree format. WhiteTimberwolf, #link("https://creativecommons.org/licenses/by-sa/3.0/deed.en")[CC BY-SA 3.0], via Wikimedia Commons]
)<octreeimg>

While chunks are typically uniform, fixed-size sections of the game world loaded and unloaded as needed, octrees offer a more dynamic approach. In some implementations, each leaf node of an octree represents a chunk, allowing for variable levels of detail within different regions of the game world. This integration enables efficient memory usage and rendering performance, especially in procedurally generated or expansive environments.
== GPU Computation #text(red)[Erik]
GPU #text(maroon)[Staffan: ref] computing is the process of utilizing the highly parallel nature of the GPU for running code. Since the GPU has significantly more processing units than the CPU @princeTonGPU, it can be utilized to write highly parallelized pieces of code to solve certain programming problems. These programs that run on the GPU are often referred to as compute shaders @UnityComputeShaders.

== Previous works #text(red)[William klar]
#text(maroon)[Staffan: Gå igenom olika exempel och koppla dom till begreppen som introducerats tidigare? 2.6.4. gör detta bra (kanske går det att göra bättre)]
Several existing games and research projects provide a foundation for this work, demonstrating both the potential and the challenges of procedural planet and solar system generation and simulation:

=== Minecraft #text(red)[Jacob klar]
While not focused on planetary systems, Minecraft @minecraft:2009 demonstrates the power of procedural generation in creating vast and varied landscapes using noise functions (explained in @background-noise-ref). In Minecraft, noise is used to procedurally create terrain features such as mountains, valleys, and cave systems. This approach results in endless, unique environments for the player to explore.

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
Outer Wilds @OuterWilds0 is a space exploration and adventure game that bears a small resemblance to this project. Unlike the other works mentioned, which are related through their use of procedural generation, Outer Wilds features entirely hand-crafted content. Its relevance to this project instead lies in its approach to physics. In Outer Wilds, all physics interactions are computed in real time, with no pre-defined behaviors. For instance, planetary motion is governed by a modified version of Newton's law of gravitation, and all velocities are dynamically calculated during gameplay @outerwilds1.

#figure(
  image("images/PreviousWorks/outerwilds_mech_3.jpg", width: 65%),
  caption: [Map of the Outer Wilds solar system]
)

=== Exo Explorer
Exo Explorer @exo_exporer:2023 is an earlier bachelor thesis project, also from Chalmers, directly addressed the challenge of procedurally generating solar systems using the Unity engine. The project utilized Perlin noise @perlinnoise:1985 and the marching cubes algorithm @marchingcubes:1998 to create planet terrain featuring forests, lakes, and creatures with basic simulated needs (hunger, thirst, reproduction). 

Exo Explorer served as a valuable source of inspiration for this project, demonstrating techniques of procedural generation, optimization, etc, that this project aims to explore as well. Whilst aiming to delve deeper into other aspects such as the complexity of the simulated physics, performance, and exploration; especially with a greater focus on simulating several solar systems at the same time for the user to explore.

#figure(
  image("images/PreviousWorks/exoexplorer.png", width: 65%),
  caption: [Cover photo from the #link("https://github.com/Danilll01/Kandidatarbete2023?tab=readme-ov-file")[#underline[project's repository]]]
)

= Method and Planning #text(red)[Fixa tempus till typ was planned to] #text(maroon)[Staffan: Planning bara funkar nog här också. För att en del av planning är vilka metoder ni tänkt använda.]<Method>
This section describes the methodology and planning behind the project. It presents the chosen workflow, the intended features, selected tools and technologies, as well as considerations related to societal and ethical implications.

== Workflow <Workflow>
Development was planned to follow an Agile @Agile101 adjacent #text(maroon)[Staffan: få med i stycket varför det är adjacent] workflow, meaning that the work were to be divided into "sprints" #text(fuchsia)[_hur långa?_] with iterative task refinement. Task prioritization and addition of tasks to the backlog was to be done during the end of the week before the weekly supervisor meeting. Task management involved tracking various states for each task (on the Kanban board @kanban), including "blocked", "todo", "in progress", "in review", and "done". All labels are self-explanatory except for "blocked"; tasks categorized under this label cannot be worked on before prerequisite task(s) are finished. 

== Git <Git-section>#text(red)[Jacob klar]
During the development process, the version control system Git @git-version-control was utilized in conjunction with GitHub @github. Additionally, the GitHub repository served as a platform for task management by employing a Kanban board @kanban to facilitate tracking of task assignments and progress.

The projects standard workflow for Git and GitHub involves maintaining each feature within a dedicated branch. GitHub’s Kanban board allowed us to associate branches with specific tasks. Moreover, acceptance criteria were established for each task on the Kanban board. Once all criteria were met, a pull request was created to merge the changes into the main branch. Before finalizing the merge, at least one other team member was required to review the code and the feature. This review process served both as a quality assurance measure and as an opportunity to provide feedback.

== Godot #text(red)[Jacob klar, William kollat lite]
The Godot game engine #text(maroon)[Staffan: referens] is the engine that was chosen for this project. Godot is a free and open-source game engine. It employs a node-based system that enables modular and reusable component design. The engine officially supports multiple programming languages, including GDScript, C\#, and C++. @GodotFeatures #text(red)[vad pekar referensen till?] Furthermore, community-driven extensions, detailed in @GDExtension, expand language compatibility beyond these officially supported options to languages such as C++ and Rust.

As mentioned, everything is built using what are called Nodes. A *Node* is a fundamental building block for creating game elements, and it can represent various components such as an image, a 3D model, a camera, a collider, a sound, and more. Together, nodes form a *Tree*, and when you organize nodes in a tree, the resulting assembly is called a *Scene*. Scenes can be saved, and reused as self-contained nodes, allowing them to be instantiated in different parts of the application @godot-nodes-and-scenes. For example, a Player character might consist of multiple nodes, such as an image, collider and camera. All grouped together, they form a Player Character Scene, which can then be reused wherever needed.

== Benchmarking and Performance #text(red)[Jacob klar] <benchmarking-and-performance-ref>
When developing real-time applications such as simulations, video games, or other computer applications, maintaining responsiveness and stability during runtime is essential for the user experience. A common metric to measure the performance of any such application is Frames Per Second (FPS), which is the amount of rendered images (frames) that are displayed each second. Higher and consistent FPS is desirable for a stable experience, as well as reduced visual artifacts, and improved system latency from when a user inputs, to it being represented on the display @nvidia_fps.

However, FPS alone does not always provide a complete picture of performance. Instead, the time it takes to render each frame (frame times) is considered instead. Frame times reveal inconsistencies during runtime, such as brief momentary lag at computation heavy moments. These moments may be overlooked in average FPS values, while being detrimental to the user experience. Metrics such as 1% lows and 0.1% lows of FPS have become common @gamers_nexus_dragons_dogma_benchmark to expose these worst-case scenarios, this corresponds directly to capturing the slowest (highest value) 1% highs and 0.1% highs of frame times @nvidia_frametimes @techreport_inside_the_second.

The disparities between the 3 values of: the total frame time average, the 1% highs, and the 0.1% highs, are what is important. Reducing the disparities between each other is what is crucial for an overall stable user experience. Gamers Nexus@gamers_nexus_youtube_fps_lows mentions that disparities between frames of 8ms or more, are what is starting to become perceptible to the user.

All in all, the project was planned to put emphasis on maintaining consistent frame times, rather than high FPS, more precisely:
- Keep the disparities between the frame time average, its 1% highs, and its 0.1% highs, to a maximum of 8ms.
- Maintain an average 30 FPS (frame times of 33.33ms), when the program is not experiencing its frame time 1%, or 0.1% highs.

This, on a dedicated benchmarking computer (denoted as *PC-1*) of the following specifications:

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

Benchmarking will occasionally be performed on other machines, and their results may not directly reflect performance on the primary benchmarking computer. If benchmarks are performed on a different machine, it will be explicitly noted. Even then, comparisons before and after changes, in performance, can still offer meaningful insights into a relative improvement or degradation on the primary benchmarking computer. Any other utilized computers are: ...

#text(red)[Lägga i typ appendix istället? ->] 

#align(center,
  grid(
    columns: 2,
    gutter: 5pt,
    [
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
    ],
    [
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
    ],
  )
)



== Planning #text(red)[Erik KLAR]
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

=== Task #text(red)[ERIK KLAR]
This chapter outlines the key tasks identified during the planning phase of the project. These have been categorized into four main areas: _procedural generation_, _physics simulation_, _exploration_, and _optimization_. Each category encompasses specific objectives and implementation considerations necessary to achieve the project’s goals.

_*Procedural Content Generation*_

All content in the program was planned to be generated procedurally. Furthermore, to ensure determinism and reproducibility, all procedural content was designed to be generated using a fixed seed.

_*Planets*_

It was decided that the planets should primarily be generated using different types of noise. The planets should also be given shape as explained earlier using the marching cube algorithm. Furthermore, the planets should be generated to be aesthetically distinct from on another.

_*Solar Systems*_

The solar systems were planned to be generated using different randomized parameters such as:
- Number of planets in the system
- Orbit of celestial bodies.
- Attributes of celestial bodies 
  - Mass
  - Rotation
  - Size
Certain parameters, such as orbital radius, mass, and velocity, were planned to be fine-tuned manually to ensure system stability. This approach would allow for the possibility of generating coherent systems in real time.

_*Physics Simulation*_

An accurate gravity simulation was a central requirement for achieving realistic physical behaviors. Different gravity systems were planned to be utilized for different purposes. For example, the player system could utilize the built in physics engine to simulate gravity on planets, while the celestial bodies and solar systems could utilize a manual implementation that is better suited for them.

_*Exploration*_

An ability to explore the generated content needed to be implemented. Multiple solutions were discussed, but since the main purpose wasn't to provide an engaging game play experience, it was decided that a simple camera controller would be prioritized, with the option to add more complex features if time allowed for it. Additionally, functionality for inter-solar system traversal needed to be implemented to allow for full exploration possibilities of multiple solar systems.

_*Optimization*_

The aim is to construct a real-time application, and thus, optimization techniques are increasingly important. Inefficient algorithms and resource management will eventually lead to performance bottlenecks. To address this, proven techniques and smart solutions were planned to be explored throughout all parts of the project.

=== Features <features>
After specifying the main objectives and implementation considerations, the project's features were identified and prioritized using the MoSCoW analysis method @moscowprio:2018 (see #ref(<MosCow>)). This was done in order to better understand which features to prioritize and which to save for later.

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
=== Timeline #text(red)[ERIK KLAR]
Based on the identified tasks and features, a timeline (see #ref(<Gantt>)) for the project was created to easily follow the projects progress as time went on. It included work on the minimal viable product (MVP), development, writing of the report, and relevant deadlines.

#figure(
  image("GANTT.png", width: 100%),
  caption: [Gantt Chart]
) <Gantt>

== Societal and Ethical aspects #text(red)[ERIK, Jacob klar]

Two main points of discussion were identified during the planning phase: How procedural content generation (PCG) affects game designers, particularly level designers, as well as its impact on players.

As PCG and AI advance, there's a risk that it could replace human developers. While PCG reduces cost and development time #cite(<computers13110304>), the concerns that it may become proficient enough to replace human creativity are still present. An example for this is when the Swedish game company Mindark announced plans to fire half of their employees, primarily world builders, in favor of AI-driven content generation#cite(<MindarkAftonbladet>)

For players, PCG must be interesting enough, and playable, to not negatively affect players. Games containing PCG are at risk of containing repetitive content, which may influence a player's sense of immersion or reduce re-playability. An example is No Man’s Sky, where the procedurally generated planets felt overly repetitive and basic @pcgchallanges:2017. Additionally, PCG can also create environments that hinder gameplay, such as untraversable terrain, negatively affecting the overall experience.

= Process #text(red)[Erik, Jacob klar]
This section outlines the process for creating the various components that comprise the project. Each subsection represents a step in increasing scale - starting from the planet-scale, focusing on unique terrain generation and other planetary features, expanding to the system-scale organization of celestial bodies and their orbital physics, and finally reaching the galaxy-scale distribution of stars.

Unless stated otherwise, all figures shown in this section were produced by ourselves.

== Planet Generation #text(red)[ANTON] <planet-gen-ref>
This section will describe the process how the planet generation was implemented.

=== Height-map planets #text(red)[ANTON, ERIK, ANTON, ERIK KLAR]<heightmap-planets>
The first planets to be constructed were the height-map planets. These planets were created by mapping a cube onto a sphere and displacing the vertices using height map values (@height-maps) to create variation in the terrain elevation. Additionally, a simplistic planet shader was implemented to add visual interest by coloring the planets based on their height relative to their lowest point. 

While height maps were sufficient for generating simple planets, generating more complex terrain, such as overhangs or caves, required a different approach to be implemented. The marching cubes algorithm @marchingcubes:1998 was chosen for this purpose during the planning stage.


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

=== Transitioning from height-maps to marching cubes #text(red)[ANTON, WILLIAM, ANTON, WILLIAM KLAR]<transition>
When transitioning from height-maps to marching cubes, the method of generating the planets needed to change from a cube mapped onto a sphere to an isosurface, with each point containing an iso-value, as shown in @mct1.

The marching cubes algorithm was used to implement this approach, following the method outlined in @marching-cubes. It iterated through all points in the scalar field, identified neighboring points, compared their stored iso-values to a threshold, and computed an index used to retrieve the corresponding polygon from a lookup table (@mc15). These polygons were then used to construct the final mesh. @mct2 shows the result of this process applied to the scalar field shown in @mct1.

#text(red)[Note to self: ändra alla "noise-value" till iso-value och 3d-matrix till scalar field!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! OCH BYT TILL PCG]

//#figure(
//  image("images/PlanetNoise/noise.png", width: 160pt, height: 160pt),
//  caption: [Cube of points]
//)<fig:noise-cube>
#align(center,
  grid(
    columns: 2,
    gutter: 30pt,
    [
      #figure(
        image("images/MarchingCube/mct1.png", width: 200pt, height: 180pt),
        caption: [Scalar field input for Marching Cubes, where green represents inside the iso-surface and red outside]
      )<mct1>
    ],
    [
      #figure(
        image("images/MarchingCube/mct2.png", width: 200pt, height: 180pt),
        caption: [Marching Cube generated mesh from the scalar field]
      )<mct2>
    ]
  )
)

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
              float thirdOctave = sin(x) + sin(2*x)*0.5 + sin(4*x)*0.25;
```
$("octave")_0 = sin(x)$

$("octave")_1 = sin(x) + 1/2sin(2x)$

$("octave")_2 = sin(x) + 1/2sin(2x) + 1/4sin(4x)$

$("octave")_n = sum(i=0)^n 1/2^i sin(2^i x), n in NN, 0 <= n < "octaves"$



#text(red)[matte eller kod?]

The lower octaves have the same appearance as the higher octaves but on a smaller scale, due to fBm being self similar #text(maroon)[Staffan: är "self similar" förklarat på något annat ställe? ref?]. This property is useful when generating natural-looking terrain because it ensures that the detail will scale consistently and that the result will blend well together.

The implementation of fBm in this project was straight forward, and instead of using sine-waves, the 3D noise from before was used:

```cs
Vector3 currentPosition = new Vector3I(x, y, z);
float distanceToCenter = (centerPoint - currentPosition).Length();
float distanceToSurface = (float)radius - distanceToCenter;
points[x, y, z] = Fbm(distanceToSurface, currentPosition, fastNoise);;
```

In the fBm function, there is a single for-loop which, for each octave, transforms the previously used point-value by adding noise with increased frequency and decreased amplitude. Lacunarity is the amount that the frequency should be changed by each octave, and persistence is the amount the amplitude should be multiplied by each octave. Typical values for these are ```typst lacunarity = 2``` and ```typst persistence = 0.5``` due to generating natural-looking terrain @quilez2019fbm. However, both of these parameters are randomly chosen based on logic later described in @proc-gen in order to add more variation to the planets; to broaden the types of planets that can be generated.

#text(red)[KANSKE TA UPP SEN - in the papers, a hurst exponent is talked about... (main part)... this is related to the persistence which is used here. Commonly 2^(-h) (h is [0,1]) is used and h=1/2 => regular brownian motion.]

#box[
  #show figure: set align(left)
  #show figure.caption: set align(center)
  #figure(
        algorithm({
          import algorithmic: *
          Function("FBM", args: ("valueToModify", "position", "noise"), {
            For(cond: [$i$ *in* $0..$octaves-1$$], {
              Assign[valueToModify][noise.*Get3DNoise*(frequency $times$ position $plus$ offset) $times$ amplitude]
              Assign[amplitude][amplitude $times$ persistence]
              Assign[frequency][frequency $times$ lacunarity]
              Assign[offset][offset $plus$ smallRandomOffset]        
            })
            Return[valueToModify]
          })
        }),
      caption: [Pseudo-code for fBm (fractional Brownian motion)]
  )<fbm-code>
] 

#text(red)[HUR GÖR MAN SÅ ATT CAPTION HAMNAR UNDER KODEN?!!!!]

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
  _Note_: Between @noise-planets and @fbm-planets, interpolation was introduced to the marching cubes algorithm, which caused the planet terrain in @fig:fbm-1 and @fig:fbm-2 as well as _all subsequent figures_ to appear smoother compared to the terrain depicted in @noise-planets. #text(purple)[Är detta bra?]
]



There where some issues with these planets however, as can be seen in the center of @fig:fbm-2, where sometimes the noise function together with the fBm algorithm can cause the boarders of the planet to become part of the planet geometry in the marching cubes algorithm, which can make flat areas on the surface of the planet, or just cut of entire areas.

This issue was later solved by introducing a fall-off parameter that reduces values closer to the edge:

```cs
            falloffRatio = distanceToCenter / radius;
            falloff = falloffRatio * falloffRatio * falloffStrength;
            points[x, y, z] = fBmValue - falloff;
```

@fig:fbm_falloff_0 depicts a particularly bad example of the discussed problem and @fig:fbm_falloff_8 and @fig:fbm_flaoff_32 shows how the planet transformed with different values of the fall-off strength. The optimal strength had to be 

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


=== Procedural planet generation #text(red)[ANTON KLAR ERIK] <proc-gen>
The next step after creating the planet generation, was to procedurally generate the planets at run-time. This was done by manually experimenting with the fBm parameter values until ranges that produced visually satisfactory results (as judged by the developers) were identified. Then, the parameters was randomized, according to the logic presented in the following pseudo-code:

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
However, it was found that this method was non-deterministic, meaning that on subsequent visits to the same solar system, the planets would not look identical to those seen during the first visit. To address this issue, individual planet seeds derived from the planet's solar system seed were introduced. #text(red)[ Behövs denna delen för att förstå att det blev deterministiskt? The following pseudo-code shows how the system seed and the planet's starting position is used to calculate a new planet seed (using the method described later in @seed-ref) which is then used to update the seed of the ```typst Random``` variable from earlier:
```cs
    Random random = new Random();
    int planetSeed = GenerateSeed(systemSeed, planetPosition)
    random.seed = planetSeed;
```
]
=== Coloring the fbm planets #text(red)[ERIK KLAR]
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

== Optimizing the Planet Generation #text(red)[William Klar ERIK] <planet-optimize-ref>
Replacing height maps with marching cubes in mesh generation significantly increases computational demands due to the added dimensionality. This section describes how the planet generation was optimized in order to address this challenge.

=== Compute Shader #text(red)[William klar, Jacob klar]
#text(blue)[
The first optimization step involved moving the marching cubes from the CPU to a compute shader on the GPU. Unlike the standard rendering pipeline, compute shaders are standalone programs invoked directly by the CPU, with custom-defined inputs @openglcs.

The main motivation for this shift was to leverage the GPU’s strong parallelization capabilities. Given that the marching cubes processes each scalar independently, it inherently lends itself to parallel execution, making the GPU well-suited for this task.
]

#text(red)[
*Förslag ^*
  
#strike[The initial optimization step involved transitioning the marching cubes generation process from the CPU to a compute shader.

Unlike the standard rendering pipeline, a compute shader operates independently and is invoked directly by the CPU. In contrast to other shader stages in the rendering pipeline that follow a clearly defined input-output structure, compute shaders utilize an abstract input model that is defined by the user. @openglcs

The primary motivation for employing a compute shader was the significant parallelization #text(red)[capabilities] #text(maroon)[Staffan: avstavning på?] offered by the GPU. As previously noted, the marching cubes algorithm iterates through a scalar field, using the eight neighboring points at each step to construct a polygon. Since each iteration is largely independent of the others, the algorithm is inherently parallelizable\u{2014}an area in which GPUs excel.]
]

#text(blue)[
After transitioning the marching cubes algorithm to a compute shader, this GPU-based mesh generation approach was tested against its CPU counterpart. The testing involved feeding identical scalar fields to both the CPU and GPU #text(red)[implementations] #text(maroon)[Staffan: avstavning på?] of the algorithm and measuring the time required to generate the resulting mesh. The tests were performed across a range of different sizes to ensure broader applicability of the results.
]

#text(red)[
*Förslag ^*

#strike[After transitioning the marching cubes algorithm to a compute shader-based approach, a compute shader was successfully implemented to generate a mesh using this algorithm. Following the implementation, performance testing was carried out to assess whether GPU-based mesh generation offered improved efficiency compared to the CPU-based approach.

This testing involved feeding identical grids of data points to both the CPU and GPU #text(red)[implementations] #text(maroon)[Staffan: avstavning på?] of the algorithm and measuring the time required to generate the resulting mesh. The tests were performed across a range of different grid sizes to ensure broader applicability of the results.]
]

#text(blue)[
Surprisingly, the GPU approach performed worse (see blabla (*ta med en figur/table som visar någon benchmark*)). This is likely due to overhead from buffer setup and data retrieval from the buffer, a common bottleneck in compute shader workflows. Additionally, the triangle buffer was configured for the worst case polygonization (up to five vertices per polygon(s)), which added to retrieval time. 

In the end, this approach did not achieve the intended reduction in planet generation time; however, as alternative methods remain available, the focus shifted to exploring a different solution.
]

#text(red)[
*Förslag ^*
  
#strike[Contrary to initial expectations, the GPU implementation demonstrated lower performance than its CPU counterpart. It is hypothesized that this outcome is primarily due to the overhead associated with buffer setup and retrieval, which is a known bottleneck in compute shader workflows. Additionally, the triangle buffer was configured to accommodate the worst-case scenario in mesh generation, assuming that each voxel could produce up to five polygons, thereby increasing retrieval time.]
]

#text(red)[Vi behöver nog inte nämna Unity här?] #text(purple)[_ sant _]

#text(red)[#strike[Furthermore, unlike Unity—which allows for indirect mesh creation directly within compute shaders to mitigate buffer-related overhead—Godot lacks such functionality. As a result, buffer retrieval must be performed manually, which introduces additional inefficiencies within the engine's pipeline. In the end, this approach did not achieve the intended reduction in planet generation time; however, as alternative methods remain available, the focus shifted to exploring a different solution.]]

=== Worker Thread Pooling #text(red)[William klar] <worker-thread-pooling-ref>
An alternative approach involved partitioning the workload across multiple threads. Specifically, in addition to the main thread, a dedicated thread was introduced to handle planet generation requests. Previously, all operations were executed on the main thread, including a loop responsible for generating each planet during solar system creation. This process led to performance issues, as the computationally intensive planet generation caused noticeable stuttering; when the function to create a planet or solar system was invoked, the frame could not advance until the corresponding meshes had been fully generated. By offloading the generation tasks to a separate thread, the main thread could simply dispatch requests and proceed without delay. 

#text(blue)[
  To further optimize this two-threaded strategy, a thread pool is introduced to distribute the workload of planet generation across multiple threads, rather than relying on a single thread. A thread pool @threadpool2 works by pre-allocating a set number of threads at startup. When a task, like planet generation, is submitted, it joins a queue, and an available worker thread executes it. This approach improves efficiency and performance by reusing threads, thereby avoiding the substantial overhead associated with creating new threads for each task.
]

#text(red)[#strike[To further enhance this multi-threading strategy and reduce the overhead associated with repeatedly creating and destroying threads, the planet generation system was transitioned to use a thread pool. Since creating threads incurs considerable overhead and is relatively resource-intensive, it is desirable to minimize this cost—an objective that thread pools are designed to address.

A thread pool functions by allocating a predefined number of threads at startup; in the context of Godot @threadpool2, this initialization occurs during project startup. When a task—such as planet generation—is submitted, it is added to a task queue. One of the pre-allocated threads (commonly referred to as workers) retrieves the task from the queue, executes it, and then proceeds to the next available task. This approach eliminates the need to create new threads for each operation, thereby improving efficiency and performance. @threadpool1
]]

This multi-threaded adjustment significantly reduced stuttering between frames and allowed for a smoother experience. #text(red)[_Kanske visa någon slags benchmark för att visa att det förbättrades? (Dock så kanske lite svårt eftersom det ända som har hänt är att planet generation är på en annan tråd?)_]

=== Chunking & Level-of-detail #text(red)[William klar]
#text(red)[#strike[
An additional optimization technique for planet generation involved implementing a chunking system. Chunking, as previously described, partitions data into equally sized segments. However, to further enhance performance, it was necessary to integrate a level-of-detail (LOD) mechanism. In this context, LOD entails reducing mesh complexity for distant planetary regions by using fewer data points during mesh generation—particularly relevant when employing the marching cubes algorithm. This reduction improves performance by enabling faster loading of less detailed planetary areas.

To support both chunking and LOD, an octree data structure was adopted. As referenced in #ref(<B-octree>), an octree recursively subdivides space into hierarchical nodes. Each leaf node in the octree represents a chunk of the planet, with the depth of the node determining the level of detail—shallower leaves correspond to lower resolution. Subdivision is driven by the player's proximity: as the player approaches a region, the corresponding node subdivides into eight higher-resolution child nodes, increasing local mesh detail dynamically. This approach ensures that only regions near the player are rendered in high detail, significantly improving efficiency. In summary, the octree effectively addresses both chunking and LOD requirements in a unified structure.

The implementation initializes an instance of the Octree class with a size equal to the planet's diameter. Mesh generation is managed by the OctreePlanetSpawner class, where a resolution variable defines the number of data points per chunk (e.g., a resolution of 32 results in $32^3$ data points). @fig:octmc1 illustrates the octree planet structure with visible chunk outlines.

Subdivision occurs based on the player’s proximity to leaf nodes: when the player approaches within a set distance, the Octree subdivides the node up to a predefined MaxDepth, generating higher-resolution meshes via the OctreePlanetSpawner and disabling the parent mesh. When the player moves away, subdivisions are removed, and the parent mesh is re-enabled. The root node is never removed.

While the implementation successfully incorporates chunks and varying levels of detail (LODs), a limitation inherent to the marching cubes algorithm when used with differing LODs is the occasional appearance of gaps at the borders between chunks of disparate resolutions (as partially evident in @mct2). This issue arises because higher-resolution chunks capture more points along the isosurface, potentially revealing dips that may be overlooked in lower-resolution chunks.

There exists solutions to fix it but due to time constraints we were unable to implement them.]]

#text(blue)[
  A chunking system was implemented to optimize planet generation. To further boost performance, a level-of-detail (LOD) mechanism was added, reducing mesh complexity for distant planetary regions by using fewer scalar values, which is especially important when using the marching cubes algorithm. This speeds up loading as rendering less detailed areas is quicker.

  An octree data structure was employed to support both chunking and LOD. An octree, as mentioned in @B-octree, recursively divides space into nodes, where each leaf will now represent a planet chunk. Node depth determines the detail level—shallower leaves have lower resolution and vice versa. Subdivision is driven by player proximity: as the player approaches, the node subdivides into higher-resolution child nodes, increasing detail. This dynamic adjustment ensures that only nearby regions are rendered in high detail, improving efficiency.
  
  The implementation works by having an _Octree_ class, which is initialized with a size matching the planet's diameter, while the _OctreePlanetSpawner_ class manages mesh generation. A resolution variable sets the number of scalar values per chunk (e.g., $32^3$ for a resolution of 32). As the player nears a leaf node, it subdivides, unless it is at a predefined MaxDepth (to avoid infinite subdivisions), generating higher-resolution meshes while disabling the parent mesh. Moving away reverses this process, with subdivisions removed and the parent mesh restored. The root node always remains intact.
  
  However, a limitation of the marching cubes algorithm with varying LODs is occasional gaps between chunks of different resolutions (as partially evident in @mct2), as higher-resolution chunks capture more surface points while lower resolution might miss them. Although solutions exist to fix this issue, time constraints prevented their implementation and thus were not addressed.
]


#align(center,
  grid(
    columns: 2,
    gutter: 55pt,
    grid.cell([
      #figure(
        image("images/Octree/octmc1.png", width: 200pt),
        caption: [Octree planet outlining the chunks],
      )<fig:octmc1>
    ]),
    grid.cell([
      #figure(
        image("images/Octree/octmc2.png", width: 200pt),
        caption: [Octree planet showing the varying LODs],
      )<fig:octmc2>
    ]),
  )
)

=== Reducing stuttering??
While the mesh generation was fast at this point in the project... 
Planet data point multithread and moons not use old planets

== Planetary Features #text(red)[ERIK Klar]
This section describes process of creating the additional planetary features including: surface elements, oceans, and atmospheres.

=== Surface Elements #text(red)[William Klar]
In order to enhance the variety of the planet's surface details, it was decided to incorporate elements such as grass, bushes, trees, and oceans. This addition aims to increase the diversity of the planet's landscape. 

The implementation structure for surface details is primarily based on the work presented in the blog _Population of a Large-Scale Terrain_ @surfacedetails1, in which the author classifies surface elements into two distinct categories: *details* and *features*.

The _details_ category includes elements such as grass, which are rendered in close proximity to the player. Precise positioning of these elements is not critical, as minor discrepancies typically go unnoticed due to their abundance and the player's limited focus on individual instances (e.g., the position of a single blade of grass).

In contrast, the _features_ category encompasses elements like trees. These are intended to be visible from a distance and require consistent placement, as irregularities in their positions are more easily perceived and can negatively impact the visual coherence of the scene.

For both categories, it remains important to ensure that the generated elements are aligned with the normal of the underlying mesh. However, due to their differing visual and functional requirements, each category necessitates a distinct implementation approach.

*Surface Details (Grass)* 
#text(red)[Räknas detta som en till rubriksnivå? även om inte explicit skriven]#text(blue)[Jo det tror jag... Vi skulle nog egentligen ha det som "4.3.1 Grass", och baka in nuvarande 4.3.1 in i 4.3 Planetary features. Potentiellt ta bort mycket av nuvarande 4.3.1 för att minska ord. /Jacob]

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
One limitation of this method is that faces with relatively small areas may yield fewer than one instance, resulting in no surface detail being rendered on those faces. This effect is illustrated in @fig:simplegrass5 and @fig:simplegrass6 using a basic sphere mesh. However, for the generated mesh in this project, face size variation is significantly less pronounced than in @fig:simplegrass3, making this limitation acceptable for the current purposes.
#grid(
  columns: 2,
  gutter: 0.75cm,
  grid.cell([
    #figure(
    image("images/Grass/grasss5.png", width: 180pt),
    caption: [Density-Based Grass Spawning on a sphere]
  )<fig:simplegrass5>
]),
  grid.cell([
    #figure(
    image("images/Grass/grass6.png", width: 180pt),
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

To achieve a more spatially uniform distribution of points, the sampling method was subsequently replaced with Poisson-disc @bridson2007fast sampling. Unlike uniform sampling, Poisson-disc sampling ensures that each point is separated by a minimum distance $r$, thereby avoiding clustering and producing a more even distribution of samples.
The algorithm used follows the method described in _Fast Poisson Disk Sampling in Arbitrary Dimensions_ @bridson2007fast, which operates as follows:

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


=== Atmospheres #text(red)[ERIK, JACOB klar Erik Kollat]

Atmosphere development began with research into existing solutions, resulting in two approaches: a simpler method by Martin Donald @martin-donald-atmosphere and a more physically accurate one by Sebastian Lague @sebastian-lague-atmosphere. Both implementations utilize a post-processing shader on a cube with flipped faces.

The simpler version was implemented using ray-sphere intersections to create a transparent, uniformly colored, sphere around the planet (see @ray-sphere-atmosphere). The color remains uniform since it does not account for the sun's position. Attempts to improve shading involved calculating the dot product between each vertex normal and the sun ray directions, meant to influence the resulting color. However, due to issues with different coordinate spaces, and fetching the sun's position, this was unsuccessful and caused a shift towards the more realistic approach.

#figure(
  image("images/Atmosphere/basic_atmosphere.png", width: 50%),
  caption: [Implementation of a simple atmosphere]
) <ray-sphere-atmosphere>

The second iteration aimed for a more physically accurate atmosphere involving Rayleigh scattering. Rayleigh scattering is a physical phenomenon which describes how light interacts with particles smaller than its wavelength @RayleighScattering, allowing for atmospheric light scattering, density falloff with altitude, and sunsets. The algorithm roughly works by approximating light scattering along a ray cast from the camera through the atmosphere, using a series of sampling points for optical depth and scattering calculations.

The color is based on a 3D vector representing different light wavelengths corresponding the different parts of the visible light spectrum. However, basing it on only Rayleigh scattering limited the achievable color options. For example, a red Mars-like atmosphere was not possible through this method alone. To address this, one of the wavelengths was set lower than the others, amplifying the scattering of that wavelength, and thereby altering its color. Finally, a set of preset wavelength vectors were created and are chosen at random as the planets are generated. See @AtmosphereColors.

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

Initially, atmospheric rendering was performance-heavy due to costly light scattering calculations. Two solutions were considered: offloading calculations to a compute shader, and a LOD system. The latter was chosen for its simplicity, and is done by dynamically reducing scattering and optical depth samples with distance to the player.

Originally set to 30, the number of sampling points along the rays traveling through the atmosphere caused significant performance drops.After testing, a sample count of 10 was found to balance visual fidelity and performance effectively, reducing average frame time from 132.2 ms (at 30 samples) to 18.56 ms in a single-planet benchmark on PC-3 (see #ref(<pc-3-specs>)). Furthermore, with the LOD in place, the amount of sampling points reduce from the set maximum value, downwards to 1, as the distance to the player increase. This further increases performance since all atmospheres in a system does no longer render at full quality at the same time.

== Player Controller #text(red)[ERIK KLAR] <player-controls-ref>
The player controller was initially implemented as a flying camera for free exploration of the galaxy, without collision or surface interaction. After terrain generation was completed, support for planetary landings began it's development. This required simulating local gravity, surface-aligned movement, and jumping.

Planetary gravity fields were implemented using an Area3D node with a spherical collision shape (see #ref(<Area3D>)). This node allows any physics body that enters it's collision shape to inherit the gravity direction and strength set by the Area3D. The gravity direction is calculated by subtracting the planet's world position with the player's world position. This is updated each physics process to allow the direction to always point towards the planet center. 
#figure(
  image("images/Planet/Area3D.png", width: 50%),
  caption: [Area3D node with collision shape around a planet]
)<Area3D>
To facilitate exploration and simulate orbital behavior, the player inherits a planet’s total velocity upon entering its gravitational field, ensuring they remain in orbit with actively moving. The final step was implementing surface landing and movement mechanics.

Rotating the player while moving on the planetary surfaces was achieved by linearly interpolating the player’s basis toward a target basis with its ‘up’ vector aligned against the gravity vector. Finally the ability to jump was implemented by adding an impulse along the opposite direction of the gravity vector.

Several issues emerged during testing. At high velocities, the player could fall off planets, which was resolved by lowering the base speed. Adapting speed to planet radius was considered but deemed unnecessary. Uncontrollable bouncing, likely caused by uneven terrain at high speeds, was mitigated by adding a downward raycast to support the collision system. Furthermore, many issues were ultimately traced to planets moving at high speeds while the player was also in motion, combined with inaccurate planetary collision shapes, and possibly small planet radii.

== Physics Engine #text(red)[JONATAN, JACOB klar, Jonatan Klar] <physics-engine-ref>
Simulating the gravitational interactions within a galaxy, containing potentially thousands or millions of stars and planets, presents a significant computational challenge known as the N-body problem @Gangestad2025. The goal is to calculate the net gravitational force acting on each body at discrete time steps and use this information to update their positions and velocities over time. This section details the progression of methods implemented to tackle this problem within our project, moving from a simple baseline to an optimized approximation algorithm, and discusses the performance analysis that guided these choices.

=== Direct Summation #text(red)[Jonatan, Jacob klar, Jonatan, Anton klar] <physics-direct-summation-ref>
The most straightforward approach to solving the N-body problem is the direct summation method#text(blue)[ [källa]] #text(red)[(Jacob: Nu är jag petig men behövs det källa till att det är den mest "stragihtforward approach"? Man kanske bara kan skriva att det är "an approach" annars)]#text(fuchsia)[Jonatan: Vet inte om det behövs eftersom den senare direkt härleds från newtons lagar] #text(blue)[Anton: tror också det är bäst att inte skriva "most straightforward approach to solving" utan en förklaring till varför alla andra metoder inte är mer straightforward + en källa till det. Kanske är bättre med "In order to solve the N-body problem research was conducted.. and eventually an approach called direct summation method was discovered.. this is why it was good for this situation.." + en källa till den :P]. This technique relies directly on Newton's Law of Universal Gravitation @newton1687, calculating the gravitational force between every pair of particles in the system.

The force $arrow(F)_12$ exerted on particle 1 by particle 2 is given by:
$
  arrow(F)_12 = G (m_1 m_2)/(|arrow(r)_12|^3) arrow(r)_12 quad "where" quad arrow(r)_12 = arrow(r)_2 - arrow(r)_1
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


This requires $N(N−1)/2$ pairwise calculations per time step, resulting in a computational complexity of $O(N^2)$. Our implementation uses nested loops as illustrated in the pseudocode below:

#box[
#show figure: set align(left)
#show figure.caption: set align(center)

#figure(
  algorithm({
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
  }),
  kind: "code",
  supplement: [Code],
  caption: [Pseudocode for the direct summation algorithm]
)
]

While simple and accurate, the $O(N^2)$ complexity makes direct summation computationally prohibitive for large N within our target performance goals, necessitating an approximation method.

=== Barnes-Hut Approximation #text(red)[Jonatan, Jacob klar. phew, Jonatan Klar (-diagram)]
To efficiently simulate large numbers of bodies, we implemented the Barnes-Hut algorithm @Barnes_Hut_1986, which reduces the computational complexity to $O(N "log"N)$. The core idea is to use an *octree* @octree1 (Described in @B-octree) to group distant particles together. The gravitational influence of these groups is then approximated by treating the group as a single point mass located at its center of mass (CoM). This approximation leverages Newton's shell theorem @newton1687 and is effective when the distance to the group is large compared to the group's size.

In dynamic N-body simulations, constantly changing particle positions quickly invalidate the octree, typically requiring reconstruction each time step (physics frame). This demands an efficient construction algorithm, for which a *Morton-code-based linear octree* @gargantini1982 was chosen, enabling:


1.  *Parallelizable Steps:* Calculating Morton codes and sorting particles are trivial to parallelize.

2.  *Cache Efficiency:* Processing spatially local data sequentially after sorting can lead to better CPU cache utilization compared to pointer-chasing in traditional octree implementations.

3.  *Efficient Partitioning:* The sorted order allows for fast partitioning of particles into child nodes using binary search rather than geometric tests.

The construction process proceeds as follows:

1. *Morton Codes:* A 64-bit Morton code is calculated for each particle by mapping a particle's 3D position within the global simulation bounds to a 1D integer by interleaving the bits of its scaled coordinates. This mapping largely preserves spatial locality since nearby particles tend to have numerically close Morton codes @morton1966. This step is parallelized using `rayon` @rayon for particle counts larger than a set threshold, determined via benchmarking (see @physics-benchmarking-ref).

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

2. *Sorting:* Particles are sorted based on their Morton codes, effectively grouping spatially adjacent particles in memory. The sorting is parallelized using `rayon`, providing significant speedup.

3. *Tree Construction:* An explicit linear tree structure is built recursively from the sorted list of particles. The construction works by dividing the particle list into smaller ranges:
    - If a range contains one particle or the maximum depth is reached, a leaf node is created, and its `GravityData` (mass and center of mass) is computed.
    - For internal nodes, the range is partitioned into into 8 sub-ranges (octants). This is done efficiently by performing a binary search on the sorted Morton codes, checking the relevant 3 bits at the current depth to find the split points.
    - Recursive calls are made for each non-empty octant, and parent nodes aggregate `GravityData` from their children.
  Currently the tree-building step is run sequentially, but there's potential for parallelization.




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


After the octree is built for the current time step, each particle's acceleration is computed by traversing the tree, using the _Multipole Acceptance Criterion (MAC)_ @Barnes_Hut_1986:

1.  Compute the distance $d$ between the particle and the node's center of mass (CoM).
2.  Determine the node's size $s$ (bounding box width).
3.  If $s^2 < theta^2 d^2$ (where $theta$ is a threshold parameter), the node is far enough. Its gravitational effect is approximated using its total mass and CoM, and the traversal down this branch stops.
4.  If the node is too close:
    - For internal nodes, recursively traverse its non-empty children.
    - For leaf nodes, perform direct summation (@physics-direct-summation-ref) of all particles in that region. To avoid singularities, a softening factor $epsilon$ is used ($d^2$ replaced by $d^2+epsilon^2$).

Because each particle's traversal is an independent tree traversal, this process is trivially parallelized using `rayon`, greatly improving performance:

#box[
```rs
accelerations = (0..particles.len())
    .into_par_iter() // Only need this line to pararellize computation
    .map(|i| octree.calculate_accel_on_particle(g, i))
    .collect()
```
]

#text(red)[Diagram illustrating the BH traversal for one particle (MAC pass/fail). #text(blue)[jo det låter nice]]

=== Integration #text(red)[Jonatan Klar] <P-integration>
Finally, the calculated accelerations (whether from Direct Summation or Barnes-Hut) are used to advance the simulation state via numerical integration. Our implementation uses the Forward Euler method @euler1768integral (specifically, symplectic Euler, which offers better long-term stability for orbital mechanics @brorson_symplectic_integrators)  within the `step_time` function (`controller.rs`) to update velocities and positions based on the accelerations computed in parallel for the current time step $Delta t$:

$v_"new" = v_"old" + arrow(a) dot Delta t quad $ (```rs body.vel += acc * delta```)

$p_"new" = p_"old" + arrow(v)_"new" dot Delta t quad $ (```rs body.pos += body.vel * delta```)

This combination - parallelized efficient octree construction via Morton codes, $O(N "log"N)$ force calculation with parallel traversals, and simple Euler integration - allows the simulation of large-scale galactic systems.

=== Performance Benchmarking and Threshold Tuning #text(red)[Jonatan Klar] <physics-benchmarking-ref>
To guide optimization efforts and make informed decisions about algorithm choices and parallelization strategies, rigorous performance benchmarking was conducted on the core components of the physics engine. We utilized the `criterion` @criterion.rs Rust library, a powerful statistical benchmarking harness. `Criterion` provides several advantages over simple timing loops, including running benchmarks multiple times to gather statistically significant data, detecting performance regressions between code versions, and generating detailed reports, making it invaluable for performance analysis.

The benchmark suite was designed to measure the performance of critical functions under varying workloads, primarily different numbers of simulated bodies ($N$). Key benchmark groups included `compute_accelerations` (comparing Direct Summation vs. Barnes-Hut) and specific parts of the Morton-based octree construction like `morton_encode` and `morton_sort`. Test data (`SimulatedBody` instances) was generated consistently using helper functions like `create_bench_bodies` to ensure repeatable results across runs.


1. *Algorithm Selection Thresholds for Force Calculation:* The key results for the `compute_accelerations` benchmark group are summarized in @fig:calc-acc-bench. This graph plots the average computation time (in milliseconds, on a logarithmic scale) against the number of bodies for four variants: sequential Direct Summation (`direct/sequential`, yellow line), parallel Direct Summation (`direct/parallel`, blue line), sequential Barnes-Hut (`barnes_hut/sequential`, green line), and parallel Barnes-Hut (Morton-based octree, `barnes_hut/parallel`, red line). Each data point represents the average of 100 samples and was run on. The benchmarks were ran on PC-1 (see @pc-1-specs), so thresholds on other hardware might vary - especially regarding number of CPU-cores. This visualization was crucial for determining our performance thresholds:

    - For very small numbers of bodies ($N < 100$), the `direct/sequential` method (yellow line) is the most performant. Its low intrinsic overhead makes it ideal for these scenarios, despite its $O(N^2)$ complexity. All other algorithms show an overhead, especially the parallel ones.
    
    - As $N$ increases beyond approximately $100$, the `direct/parallel` method (blue line) surpasses sequential direct summation and also remains faster than `barnes_hut/parallel` (red line) for a significant range. This is because the parallelization of the $N^2$ calculations effectively utilizes multiple cores, and this benefit outweighs the Barnes-Hut octree construction overhead until the $N^2$ factor becomes too dominant.
    
    - The final crossover occurs at approximately $N=440$, where the `barnes_hut/parallel` method (red line) becomes the most efficient. Beyond this point, the $O(N "log"N)$ complexity of Barnes-Hut, combined with parallelism, provides superior performance over both direct summation variants.
    
  These empirical results from the benchmark graph directly informed the multi-tiered dynamic switching mechanism implemented in the physics controller.

    #box[
    ```rs
    let accelerations = match bodies_sim.len() {
            //          Algorithm                  Parallel
               ..100 => DirectSummation::calc_accs::<false>(grav_const, bodies_sim),
            100..440 => DirectSummation::calc_accs::<true>(grav_const, bodies_sim),
            440..    => MortonBasedOctree::calc_accs::<true>(grav_const, bodies_sim),
        }
    ```
    ]
    
    This strategy ensures the simulation adaptively selects the most performant algorithm variant—sequential direct sum for very few bodies, parallel direct sum for an intermediate range, and parallel Barnes-Hut for larger numbers—based on the current number of interacting bodies. The "Real-time (60Fps) threshold" line on the diagram provides additional context regarding the absolute performance of these methods.

#figure(
 image("calc_acc_bench.png"),
 caption: [
   Benchmark results for N-body acceleration calculations, illustrating the performance crossover points that informed the selection of algorithm-switching thresholds. Average time per 100 samples (ms, log scale) vs. number of bodies. Key thresholds for Direct Summation vs. Barnes-Hut, and parallelization overheads are noted.
 ]
)<fig:calc-acc-bench>

    
2.  *Parallel Morton Encoding Threshold (`PARALLEL_ENCODE_THRESHOLD`):*
    The `morton_encode` benchmark group specifically compared the performance of calculating Morton codes sequentially versus in parallel using rayon. The results are visualized in @fig:morton-bench. This graph plots average encoding time against the number of bodies, for both sequential (`morton-sequential`, green line) and parallel (`morton-parallel`, red line) implementations.
    
    As the diagram illustrates, for smaller numbers of particles, the sequential encoding is faster due to the overhead associated with initializing and managing parallel tasks. However, as the number of bodies increases, the benefits of parallel computation become apparent. The intersection point, where the parallel version starts to outperform the sequential one, is clearly visible around $N approx 4000$. Based on these empirical results, for particle counts below 4000, Morton codes are calculated sequentially, while for counts at or above 4000, the parallel `rayon` implementation is utilized to leverage multi-core processing.

#figure(
  image("morton-bench.png"),
  caption: [
    Morton encoding benchmark results demonstrating the benefit of parallelization for larger datasets. Average time per 100 samples (ms, log scale) vs. number of bodies. Parallel encoding surpasses sequential performance around N ≈ 4000, after overcoming initial Rayon overhead.
  ]
)<fig:morton-bench>


In summary, using `criterion` for systematic benchmarking was crucial for optimizing the physics engine. It provided the quantitative data needed to justify algorithmic choices and fine-tune parameters like parallelization and algorithm-switching thresholds, leading to a more performant and scalable simulation.

=== Trajectory Simulation and Visualization <P-trajectories>

To understand orbital dynamics and aid in system design, a trajectory simulation system was implemented to predict and visualize the future paths of celestial bodies. This system runs a separate N-body simulation for a configurable number of future steps (`simulation_steps`) and time increment (`simulation_step_delta`), using the same core physics logic (Direct Summation or Barnes-Hut) and semi-implicit Euler integration as the main simulation (`GravityController::step_time`). The resulting sequence of future positions for each body is stored in a `Trajectory` struct and can be rendered as colored line strip meshes (`MeshInstance3D`) in Godot. Trajectories can also be calculated relative to a central body.

Given the computational cost, especially for many steps or bodies, trajectory calculations are offloaded to a background thread managed by `TrajectoryWorker`. The main thread sends `TrajectoryCommand::Calculate` messages with `SimulationInfo` to this worker and retrieves results asynchronously using a queue-and-poll mechanism (`queue_simulate_trajectories`, `poll_trajectory_results`). This prevents the main game loop from freezing during intensive calculations. The worker is designed to process the latest request if multiple are queued, ensuring responsiveness.

The accuracy of these predicted trajectories is subject to the same numerical errors as the primary Euler integration, accumulating with the number of steps; smaller `simulation_step_delta` improves accuracy at the cost of computation. However, it was observed that less precise trajectories (larger `delta`) tended to overestimate instability, providing a useful heuristic: if a system appeared stable with coarse predictions, it was generally stable in practice.

This trajectory visualization proved invaluable for the `System Generation` process (@system-gen-ref), allowing for iterative tuning of orbital parameters to achieve stable or aesthetically desirable configurations. It served as a key diagnostic tool for debugging physics and visually confirming the immediate future dynamics of generated solar systems.


== System Generation #text(red)[Paul klar, kan behöva renskrivas lite ERIK renskrev Klar] <system-gen-ref>

  System generation involves deterministically generating stable and aesthetically plausible solar systems and their contained celestial bodies. This includes calculating orbital positions for planets and moons to ensure long-term stability, and assigning unique seeds to each celestial body for reproducible procedural generation.

  The generation of systems must balance realism, aesthetics, and stability. Generating stable systems with realistic distances lead to planets appearing too small to be visible, which deteriorates the game play experience. On the contrary, smaller distances with large planets could lead to instability, specifically for moons. The moons' orbit radius increase with the planets' radius, increasing the risk of moons interacting neighboring planets.
  
  To verify a system's stability, the previously implemented trajectories (see #ref(<P-trajectories>)) were used (see #ref(<threePlanetTrajectories>)). However, these trajectories were not entirely accurate; numerical errors accumulate over time, meaning that initially stable orbits may eventually become unstable.
  
  These errors tended to grow when bodies undergo large transformations between physics steps. The errors were reduced by utilizing a higher physics frame rate and slowing down the orbit speeds. Conversely, faster orbits could lead to a higher accumulation of errors, making stable systems appear unstable in the simulation.

#figure(
  image("trajectoriesSystem3Planets.png", width: 50%),
  caption: [Three planet system with visible trajectories.]
)<threePlanetTrajectories>
=== General Flow #text(red)[PAUL KLAR ERIK]
#text(red)[Denna delen kanske skulle kunna vara i resultatet, med tanke på att den bara säger som det är och inte beskriver processen? JAg tror typ att vi också beskriver den i kapitlet ovan] 

The input seed for the system gets added as the seed to a random number generator. This ensures that it produces the same random numbers each time the system gets generated. For each attribute of the system a value for that attribute gets randomized within an interval like so.

```ts
func generateSystemDataFromSeed(s: int):
	var r = RandomNumberGenerator.new()
	r.seed = s

func randomPlanetMass(r):
	return r.randf_range(MIN_PLANET_MASS, MAX_PLANET_MASS)
```

=== Solar System Stars #text(red)[PAUL KLAR ERIK KLAR]

  The stars were at first implemented as simple yellow spheres with constant masses and fixed positions and radiuses. Other than the colors, the constant variables would not be randomly generated as the stars would not physically interact with other stars. This could easily be changed if necessary.
  
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
  

=== Planets #text(red)[PAUL KLAR ERIK KLAR]

   To fulfill the goal of generating solar systems, planets needed to be placed into the systems along with the stars. The planets were initially assigned orbit radiuses which increased linearly with a base distance from the sun. This proved to be unstable for systems containing moons and with smaller distances between planets. After experimentation, scaling the orbit radiuses linearithmically proved to provide a better balance between aesthetics and stability. This design choice is inspired by the structure of our own solar system, where outer planets are spaced farther apart than inner ones @planetary-fact-sheet.

   

  In order for a planet to stay at the same distance from its star, it was assigned an initial velocity perpendicular to the vector in the direction of the star. The velocity $v$ scales with the star's mass $m$, the gravitational constant $G$, and inversely with the orbit radius $r$.
  
  $ |v| = sqrt(G dot m/ r) $
  
  Modifying these variables would therefore increase or decrease the planet's velocity.
  
  It was discussed whether to simulate elliptical orbits or to continue with the spherical. Elliptical orbits are more realistic and would be possible but comes with the risk of increasing system instability.
  
  Besides the placements of planets the system also randomizes the angle around the star it should be placed at, mass, and radius of planets. Unlike reality, in which mass scales with the radius and placing moons, the mass and radius are randomized and independent from each other.




=== Moons #text(red)[William KLAR (generated, textured) och PAUL KLAR (system) Erik Klar]
  
  Procedurally placing the moons followed the same procedure as the planet placement, but with the planet as the central reference point instead of the star. Only the planets from the fourth position outward were allowed to have moons, to avoid gravitational interference due to their closer proximity.

  The moons’ orbit radii increased linearly. The spacing between moons was computed by dividing the planet’s orbit increase value by a constant MOON_ORBIT_RATIO_PLANET_DISTANCE, typically set between 40 and 100. A higher ratio results in moons being closer to their planet and to each other.

  The orbital velocity and initial angle (orbit angle) were calculated in the same way as for planets. However, it uses the planet’s mass rather than the star’s.
 
  The mass of a moon is fixed to be 1/10,000 of its planet's mass, while its radius is randomly chosen within the range of $r/10$ to $r/5$, where $r$ is the planet’s radius.

  The moon's appearance was created by generating a sphere, followed by randomly positioning craters along its surface. Each vertex was then processed by iterating through all predefined craters to calculate height adjustments based on a method created by Sebastian Lague @SebLagPlanet, using the following formulas:
  ```cs
    cavity = x * x - 1;
    
    rimX = Min(x - 1 - rimWidth, 0);
    rim = rimSteepness * rimX * rimX;
    
    craterShape = Max(cavity, floorHeight);
    craterShape = Min(craterShape, rim);
    
    craterHeight += craterShape * crater.Radius;
  ```
  Here, _x_ denotes the distance from the vertex to the crater, while the constants _rimWidth_, _rimSteepness_, and _floorHeight_ control the crater shape. The calculated _craterHeight_ determines the displacement of the vertex along its normal.

  An issue that arises when using the _Max_ and _Min_ functions is that only one of the values will be utilized, which can result in the formation of harsh shaped craters (see @MoonSmooth0). To address this, a smooth minimum and maximum was employed. These functions are based on the approach described in Inigo Quilez's article _Smooth minimum for SDFs_ @SmoothMinMax.
  
  The smooth minimum function can be formulated as follows:
  ```cs
  	SmoothMin(float a, float b, float k)
  	{
  		var h = Clamp((b - a + k) / (2.0 * k), 0.0, 1.0);
  		return a * h + b * (1.0 - h) - k * h * (1.0 - h);
  	}
  ```
Here, the parameter k represents the smoothness factor, indicating the degree to which the values are smoothed.

The smooth maximum function can be derived by inverting the smoothness factor as follows:
  ```cs
  	SmoothMax(float a, float b, float k)
  	{
  		return SmoothMin(a, b, -k);
  	}
  ```
  Enabling this smoothing mechanism allows for better smoothness over craters, as illustrated in @MoonSmooth1
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
  
  To resolve this issue, triplanar mapping @TriplanarMapping was employed. This technique involves sampling the texture by projecting it from the three basis vectors (x, y, z), effectively "wrapping" the texture around the object. Since triplanar mapping is a built-in feature in Godot, it was simply enabled in the material setting. By applying both a color texture and a normal texture, the moon achieved a rocky surface with well-defined craters shown in @MoonTexture1.
  
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
      )<MoonTexture1>,
    ]
  )
)


== Galaxy #text(red)[Jacob, Anton, Jonatan, Jacob klar]
A galaxy is a massive collection of stars, gas and dust, ranging in diameters of 1500 to 300,000 light-years @galaxy-term. In the context of this project, the galaxy represents the largest scale of the simulation — a vast space populated by procedurally placed stars.

#text(red)[*SEED står nu i Glossary. Ta bort allt det här?*]
#strike()[
#text(red)[detta stycke hade nog passat mer i resultat? passar inte direkt i processen för du pratar om hur slutversionen fungerar ]A key feature of the Galaxy's implementation is deterministic, or "seeded", generation. This approach allows for the "random" values produced by a random number generator to be predetermined based on an initial seed. This is desirable since a goal of this project is to ensure reproducible and consistent generation.

#text(red)[samma här, nämner också seed mycket men inte förklarat vad det är ]#text(fuchsia)[!!!] All iterations of the galaxy utilizes an arbitrary integer seed to influence the generation of stars, with the same input seed always yielding the same galaxy configuration. #text(blue)[The term "random" is used loosely, as it refers to this described seeding process] #text(fuchsia)[_Och för att det är en dator och inte ren slump?_]


#text(red)[Se det blå ovan ^]

#strike()[
The term "randomly" is used loosely, as it refers to this #text(red)[controlled process, lite otydligt vilken process den menar ]controlled process. #text(blue)[The term "random" is used loosely, as it refers to the described seeding process of always generating the same star configuration with the same seed.] #text(red)[kanske något sånt istället?]

]

]


The following sections introduce the various iterations the galaxy underwent during development.


=== Star field #text(red)[Jacob, ANTON, Jacob, Jonatan, Jacob] <star-field-ref>
The first version of the galaxy, a 3D distribution of stars that we called a 'star field', can be seen in @star-field-img. Points were sampled randomly within a finite cube using Godot's random number generator @godot-random-number-generator, and seeding it, to determine where each star would be instantiated. The stars were constructed using a single circular mesh, as displayed in @star-img. 

#text(fuchsia)[_ Vet inte om bara random räcker, kanske måste skriva vilken sorts distr._] #text(blue)[Joo, sant. jag skrev till det ^ /Jacob]

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

=== Disc galaxy #text(red)[Jacob, Anton, Jacob, Anton, Jonatan klar] <disc-galaxy-ref>
Thereafter, a version of the galaxy that imitates the formation of a disc galaxy was created. A disc galaxy is characterized by a flat, rotating disc structure, with a greater concentration of stars at the center @disc-galaxy.

This was achieved by sampling random points from within a sphere instead of a cube, again, using Godot's random number generator @godot-random-number-generator. The distribution was also influenced by reducing the probability of a star being placed the further away it was located from the galaxy center, thus:
 - reducing vertical spread, which would mimic the flattened shape of a disc.
 - increasing the probability of stars being placed near the center, resulting in a greater concentration of stars near the center.
 
Together, this resulted in a galaxy with a disc-like distribution, as can be seen in @disc-galaxy-img. 

#figure(
  image("images/Galaxy/disc_galaxy.PNG", width: 60%),
  caption: [Disc galaxy],
) <disc-galaxy-img>

=== Skybox #text(red)[Jacob, ANTON, Jacob, Anton, Jonatan klar] <skybox-ref>

A traditional skybox was created in Blender @blender @blender-youtube to serve as a pre-rendered galaxy background. Unlike the procedurally generated star fields, it does not contain actual 3D stars, but instead imitates a dense star field using a single static image, as seen in @skybox-testing-img.

#figure(
  image("images/Galaxy/skybox_testing_environment.PNG", width: 60%),
  caption: [Skybox testing environment],
) <skybox-testing-img>

This approach was mainly used for presentation and testing purposes. Since the final goal was to use a backdrop composed of actual, explorable stars, this implementation was not intended for the final product.


=== Infinite galaxy #text(red)[Jacob, Erik, Jacob klar] <infinite-galaxy-ref>
This version is based on the original star field concept from @star-field-ref, this time, expanding infinitely in all directions rather than being limited to a confined structure. Stars were distributed procedurally using a seeded random generator. The result can be seen in @infinite-galaxy-img.

Additionally, star placement was further refined by sampling from a noise texture. This approach was used to influence clustering, creating areas of higher and lower star densities, to make the galaxy more varied and visually interesting. #text(red)[Tycker den här delen låter lite som ett resultat? Mest på grund av att det är skrivet it presens "star placement *is* now..."] #text(blue)[Tycker nog det inte är fel att ha den här. men du har rätt om hur det hade formulerats så jag ändrade lite. Se vad du tycker.]

#figure(
  image("images/Galaxy/infinite_galaxy.PNG", width: 70%),
  caption: [Infinite galaxy],
) <infinite-galaxy-img>

To support infinite exploration, the galaxy space was divided into discrete chunks. Only chunks in the player's closest vicinity are generated and rendered, while distant chunks are culled to save performance. As the player moves, new chunks are generated procedurally, giving the illusion of an endless galaxy. An example of a "Star chunk" is shown in @star-chunk-img. #text(red)[den här delen också?] #text(blue)[Jag ändrade formuleringen här också.]


#figure(
  image("images/Galaxy/star_chunk.PNG", width: 70%),
  caption: [Star chunk],
) <star-chunk-img>


=== Finite physics-based galaxy #text(red)[Jacob klar Erik] <physics-galaxy-ref>

//-Changed to multimesh here as well.
//-Star finder refactored to work with octree's, as well as for moving stars (maybe)
#text(red)[Jag undrar om inte den här delen kan vara en del av resultatet/nån diskussion också? Förstår ju varför man vill beskriva det här i processen, eftersom vi ändå har gjort den? Men det känns som att den skulle kunna passa in i nån form a diskussion eller resultat, men att man ändå nämner den i processen med kanske?]

An Infinite galaxy is a compelling concept, but applying physics to stars of an ever-expanding galaxy is not doable. Since such galaxies are infinitely vast, there is not any fixed point of reference making any attempt at global physics calculations not work.

With great advancements in the physics engine (@physics-engine-ref), an attempt to simulate physics of a finite disc-shaped galaxy was performed — no longer confined to the bounds of the solar system.

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
The Galaxy Map serves as the final connection, connecting the implementation at the galaxy-scale of the Infinite Galaxy (@infinite-galaxy-ref), to the system-scale of Solar Systems (@system-gen-ref), which in turn connects down to the planet-scale. Combining it all into a unified experience.

=== Selectable stars #text(red)[Jacob klar] <selectable-star-ref>
To enable interaction with individual stars, a new type of selectable star was implemented. Players can now hover over a star with the mouse cursor and click to select it. This was achieved by adding a spherical collision shape to the star object, which detects mouse input events within the collider. This star, and its collider, is shown in @selectable-star-img.

#figure(
  image("images/Galaxy/selectable_star.PNG", width: 60%),
  caption: [Selectable star],
) <selectable-star-img>

Infinite galaxy (@infinite-galaxy-ref) was developed to allow for distribution of any Godot Node3D scene passed to it, and not only the original star implementation. With this, the star was exchanged for the new selectable star without issues.

To indicate that a star has been selected, the star's location in space is highlighted, together with a distance measured in "Light years" (LYs). This can be seen in the center of @galaxy-map-img. In addition, the coordinates and unique seed of the star is displayed in the bottom-right corner.

=== Navigation #text(red)[Jacob klar] <galaxy-map-navigation-ref>
Two modes of transportation were been implemented for navigating the Galaxy Map.

1. Manual movement: The player can freely move around using the same player controls introduced in @player-controls-ref.
2. Fast travel: Once a star is selected, press the "->"-button in the bottom-right of @galaxy-map-img. This moves the player rapidly towards it, stopping a short distance away.

To explore the solar systems themselves, the "Explore"-button in the bottom-right of @galaxy-map-img, can be used to enter the star/solar system currently selected. When pressed, a solar system is generated based on the selected star's seed and transitions the player into it. This system exists in a separate scene from the Galaxy Map.

=== Seed #text(red)[Jacob klar]<seed-ref>
The galaxy utilizes a unique "Galaxy Seed", the same used in @infinite-galaxy-ref, to deterministically generate the placement of stars. With the implementation of explorable solar systems, a need arose to generate new seeds for each system. Were they to utilize the same seed, all solar systems would be identical.

To address this, a custom hash function was developed, allowing for the generation of unique, deterministic, seeds for each star. This function takes into account both the initial Galaxy Seed, and the X, Y, and Z coordinates of a star's position, to produce a star-specific seed. This new seed is then propagated into the stars generation algorithm, which results in unique solar systems while still ensuring deterministic consistency.

// källor för hash-functions?
// implementationsdetaljer? nja. kanske en fin bild på ngt vis som bara visar hur nya seeds genereras?

#figure(
  image("images/Galaxy/galaxy_map.PNG", width: 80%),
  caption: [Galaxy map],
) <galaxy-map-img>


=== Multi-Meshed Stars & Star Finder #text(red)[Jacob klar] <star-multimesh-and-finder-ref>
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
  caption: [Frame time metrics on PC-2 (@pc-2-specs) - Multi-mesh stars],
) <multi-mesh-performance-table>

However, this change introduced a new challenge. Since multi-mesh only instances visual meshes, and not other objects such as colliders, the stars were no longer selectable. To reintroduce star selection, two approaches were considered:

1. *Instantiate colliders at star positions*: Instantiate only a collider at each star position, but still render the meshes with the multi-mesh implementation.

2. *Ray-based selection*: Use the known positions of stars in space, and when the player clicks, cast a ray in that direction. At regular intervals along the ray, check the surrounding area for any star positions falling within a set radius of the ray.

The second option was implemented as a system called "Star Finder", which again allows for interaction with stars, despite them only consisting of a visual mesh. Achieved via ray-casting and distance checks to the ray at regular intervals, iterating through the array of star positions of the current chunk. The Star Finder can be seen in action in @star-finder-img, with the interval and radius of proximity checks (the blue spheres) are regular in order to not miss any stars. The radius of each check also increases the further from the start position it gets, to make selection of distant stars easier.

The first option would have allowed for simpler logic in star selection, but would also include a greater load on Godot's collider calculations, as well as keep the scene hierarchy filled with hundreds/thousands of instantiated colliders. The instancing of these colliders themselves would have likely contributed to performance deterioration, since the large amount of instancing of nodes were suspected to be the cause of the stuttering from the start.

#figure(
  image("images/Galaxy/star_finder.PNG", width: 80%),
  caption: [Star Finder],
) <star-finder-img>

=== Seamless galaxy #text(red)[Jacob klar]
With great improvements in optimizing planet generation, as detailed in @planet-optimize-ref, new opportunities emerged. Previously, transitioning from the galaxy scale into individual solar systems was a static process, triggered by a button click (@galaxy-map-navigation-ref), which then loaded a separate system scene. But now, system scenes could be dynamically instantiated in real-time as the player moves towards a star. This allows the galaxy to be populated by fully realized solar systems that load seamlessly during exploration. An example of multiple systems visible at the same time, can be seen in @seamless-galaxy-img.

#figure(
  image("images/Galaxy/seamless_galaxy_multiple_systems.png", width: 90%),
  caption: [Seamless galaxy],
) <seamless-galaxy-img>

By using the same Star Finder from @star-multimesh-and-finder-ref, stars could be continuously detected in a sphere around the player, and instantiated at a set distance from the player. The solar systems will then scale slightly upon approach, until it reaches its full scale.

However, even with the improvements in the planet generation, this still had great performance implications, as can be seen in @seamless-galaxy-table. With the frame time average remaining stable throughout, although with noticeable stutters during runtime as indicated by the 1% and 0.1% highs.

#text(red)[*Kör om testerna med de senaste fixarna*]
#figure(
  table(
  columns: (auto, auto, auto, auto),
  inset: 7pt,
  align: left,
  table.header([],[*Average*], [*1% high*], [*0.1% high*]),
  [From], [4.17 ms], [4.79 ms], [9.75 ms],
  [To], [4.31 ms], [20.81 ms], [132.01 ms],
  ),
  caption: [Frame time metrics on PC-2 (@pc-2-specs) – Seamless galaxy],
) <seamless-galaxy-table>

In addition, the UI received updates (seen in @seamless-systems-ui-img) to display more information about each solar system. Together with some added flair of an assigned stellar classification @britannica-stellar-classification roughly associated to its color, as well as a randomly selected star catalogue acronym, followed by an integer number @star-naming (the system's seed).

#figure(
  image("images/Galaxy/seamless_galaxy_ui_update.png", width: 90%),
  caption: [Updated Star Select UI],
) <seamless-systems-ui-img>

= Result #text(red)[ANTON KLAR]
The following chapter presents the final result of the project. First, a brief overview of the final product is provided in @result-overview. Then, in subsequent sections, a more in-depth look at the final product will be presented, in the style of a live-demonstration, focusing on the typical user experience and what can be expected of the developed application.

== Overview #text(red)[ANTON KLAR]<result-overview>
In the end, all of the must-have, should-have and the majority of the could-have features from the MoSCow table presented in @features were implemented. 

The final product includes a galaxy map where users can freely navigate between stars, where each star represents the central body of its own solar system. The galaxy map and all systems are generated deterministically, meaning that each time a user enters the same system, it will be generated in exactly the same manner as the first time. 

Entering a solar system causes its belonging celestial bodies (planets and moons) to be generated. The celestial bodies are generated procedurally using 3D-noise as well as the marching cubes algorithm, which enables complex terrain generation with overhangs. The moons have craters and a "moon-like" appearance to them and planets are colored based on their distance from the central sun; the closest planets have a "warmer" color palette while the further way planets have a "cooler" color palette. Furthermore, the planets can also have oceans and vegetation generated on their surfaces. #text(red)[oops lägg till atmosfärer också]

Finally, the end product includes a robust physics engine that is capable of updating thousands of objects simultaneously in real time.

== "Technical" ge mig ett bra namn tack -- Vi kanske inte behöver en överrubrik för detta. Vi kan bara ha "5.2 Physics Engine", "5.3 Galaxy", ... Sedan en "5.X Demo" eller nått.
abc fysik def gravitation ghijklmnopqrstuvwxyzåäö fysik
huh ""

=== Physics Engine #text(red)[Jonatan klar ish]
The Rust-based N-body physics engine (@physics-engine-ref) was successfully developed, incorporating parallelized versions of both Direct Summation and a Barnes-Hut algorithm. Benchmarks (@physics-benchmarking-ref) validated its design and optimization. A key aspect of "real-time" performance is the ability to complete computationally intensive steps within a single frame budget. For a 60 FPS target, this implies each frame, including physics calculations, should ideally complete within approximately 16.7 milliseconds. As illustrated in @fig:calc-acc-bench the parallel Barnes-Hut method (`barnes_hut/parallel`) demonstrated its capability to calculate accelerations for tens of thousands of bodies (up to approximately 45,000) within this 16.7 ms threshold, confirming its suitability for real-time simulation of large systems.

In the final application, this engine primarily governs solar system dynamics, typically performing calculations on fewer than 30 bodies. For this scale, it correctly defaults to the efficient Direct Summation method, a choice justified by its superior performance for small $N$ due to lower overhead, as detailed in our benchmark analysis (@physics-benchmarking-ref). This ensures stable local orbital mechanics. This current usage, however, is significantly below the engine's benchmarked capacity, meaning its advanced Barnes-Hut optimizations for large N are not leveraged in the primary gameplay loop. This scope was a consequence of project priorities focusing on broad galaxy exploration and diverse procedural content across multiple scales, rather than extensive inter-star dynamics in the final seamless galaxy.

The engine's scalability was nevertheless demonstrated in the "Finite physics-based galaxy" experiment (@physics-galaxy-ref), which handled 10,000 interacting stars, further confirming the engine's robustness and its potential for larger-scale simulations within real-time constraints. Thus, while currently applied to smaller-scale interactions, the physics engine stands as a performant and validated component with significant capacity for future expansions involving more complex, large-N gravitational simulations.


=== Galaxy #text(red)[Jacob] <result-galaxy-ref>

The galaxy system went through multiple iterations, with each iteration playing a part in the foundation for the final version, The Galaxy Map (@galaxy-map-ref). Within the Galaxy Map the distribution of stars from the Infinite Galaxy connects seamlessly with the Solar Systems implementation, which in turn connects to the planets. Each step in scale can be seen in the following figures, the galaxy-scale (@galaxy_map_result_1), towards the system-scale (@galaxy_map_result_2), eventually reaching the planet-scale (@galaxy_map_result_3).

#figure(
  image("images/Galaxy/Result/galaxy_map_result_1.png", width: 90%),
  caption: [Galaxy Map at the galaxy-scale],
) <galaxy_map_result_1>

#figure(
  image("images/Galaxy/Result/galaxy_map_result_2.png", width: 90%),
  caption: [Galaxy Map at the system-scale],
) <galaxy_map_result_2>

#figure(
  image("images/Galaxy/Result/galaxy_map_result_3.png", width: 90%),
  caption: [Galaxy Map at the planet-scale],
) <galaxy_map_result_3>

The seamless instantiation of system's was left as a toggleable option. By enabling it, you get the result that is seen in the images above. However, the resulting performance implications on the dedicated benchmarking computer (PC-1, Specs: @pc-1-specs) is significant, see @galaxy-map-seamless-result-table:


#text(red)[*Jonatan ->* 

var snäll och kör benchmarks för dessa. Använd branch 'star-select-ui-offset', gå till benchmark.tscn, och låt den scenen köra tills att den stängs ned av sig självt. Spara resultatet. Gå sedan till galaxy_map_benchmark.tscn och på GalaxyMap-noden klicka rutan för att stänga av seamless-galaxer, gå sedan tillbaks till benchmark.tscn och kör den igen.]

#figure(
  table(
  columns: (auto, auto, auto, auto),
  inset: 7pt,
  align: left,
  table.header([],[*Average*], [*1% high*], [*0.1% high*]),
  [From], [ ms], [ ms], [ ms],
  [To], [ ms], [ ms], [ ms],
  ),
  caption: [Frame time metrics on PC-1 (@pc-1-specs) – Seamless galaxy enabled],
) <galaxy-map-seamless-result-table>

Compared to our performance goals, as dictated earlier in @benchmarking-and-performance-ref, the average FPS is x and the greatest frame time disparity is y. This, achieves one out of two of set goals.

By disabling seamless instantiation the resulting performance is:

#figure(
  table(
  columns: (auto, auto, auto, auto),
  inset: 7pt,
  align: left,
  table.header([],[*Average*], [*1% high*], [*0.1% high*]),
  [From], [ ms], [ ms], [ ms],
  [To], [ ms], [ ms], [ ms],
  ),
  caption: [Frame time metrics on PC-1 (@pc-1-specs) – Seamless galaxy disabled],
) <galaxy-map-no-seamless-result-table>

In this instance, the average FPS is x and the greatest frame time disparity is y. This, falls within our performance goals.



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


== Result Discussion - #text(red)[TA NU]
Hur bra systemet är, uppnådde vi målen, moscow. Realism vs Gameplay experience.

=== The MoSCow table?
Vad vi lyckades med i moscow-- vilka vi avklarade gjorde

See @MosCowFinished.

Could have been more clear with our MoSCow schema. Some features were a bit unclear and some were features overlapped with each other (?), such as UI for navigating were a bit unclear and clashed a bit with galaxy traversal. Because the project was so open-ended it was difficult to get ideas out and these ideas that shaped the foundation for everything could turn out to be vague. 

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
    - #highlight(fill: rgb("#a8ffa4"))[A technique to draw complex terrain with overhangs and the like, e.g Marching cubes].
    - #highlight(fill: rgb("#a8ffa4"))[Solar systems can have multiple planets].
    - #highlight(fill: rgb("#a8ffa4"))[Some planets should have moon(s)].
    - #highlight(fill: rgb("#a8ffa4"))[Galaxy. A procedurally generated galaxy constructed out of solar systems]
    - #highlight(fill: rgb("#a8ffa4"))[Galaxy traversal. Ways of navigating between solar systems, using a UI galaxy map or traveling from solar systems in real time. Zooming out: Solar system level, solar systems in proximity, galaxy level. (examples)]
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
    - #highlight(fill: rgb("#ffbc7e"))[Vegetation]
  ],
  ),
  caption: [The projects MoSCow Table showing which tasks were finished (marked green), tasks unfinished (marked red), and tasks that were started but not finished (marked orange).],
)<MosCowFinished>



=== Balancing performance, visuals and user experience? #text(red)[ANTON]
#text(orange)[Performance vs. kvalitet/hur snyggt det är? kanske hade passat här? alltså atmosfärer samt att skapa planeter med färre punkter och sen skala upp dom. Jacob skrev tidigare om frame-times och att min hålla sig inom dippar av 8ms (alltså att minska "stutters" där fps plötsligt går ner när den annars har varit stabil etc)] #text(red)[Det tycker jag låter coolt /ERIK]

Atmospheres scatter points, planet resolution

One of the most important parts of this project in order to make it work was ... but eventually one has to make the decision of improving performance or the visuals.. at some point, improving performance becomes more difficult if you want to keep the same visual quality... These kinds of considerations had to be discussed throughout the project... The goal was still to have consistent frame-times.. 

Making the planets is one such ... Due to having to iterate through all data points in order to construct the mesh, only so much can be done.... multi-thread... The solution that was decided upon was to 

Since the amount of data points for each planet scales cubicly with the resolution, the time to iterate over these points is a major concern for the performance. A higher resolution produces more detailed and complex terrain which can make the experience ... but this comes at the cost of the performance. This cost was greatly reduced as explained during @planet-optimize-ref, but..

A key aspect of the project, from the outset, was optimization...... but at the same time, we wanted the finished product to look good. So, balancing the quality of the visuals with the performance was decided to be an important task. There are several ways this was done. 

.

Initially, the resolution of the planets directly correlated with the amount of data points within them, meaning that a planet with a larger radius required more iterations to generate. This posed a problem when scaling up the galaxy as the planets became too demanding to generate. Therefore, by separating the radius from the amount of data points (the resolution of the planet) it became possible to scale the planets without increasing their resolution.

.

To reduce stuttering when loading in a new solar system (due to the planets generating), the planet generation was offloaded to different threads (@worker-thread-pooling-ref), and to avoid planets popping in during gameplay, they were given a temporary mesh that gets replaced once their real mesh has been constructed. This was an example of where the visuals where directly impacted by the performance, albeit temporarily during gameplay. It also affected the user experience due to players having to wait for the planets to be constructed. We felt that this was a good compromise between user experience and visuals because otherwise players might get frustrated when the game freezes each time a solar system loads.



=== Balance Between Realism and Gameplay #text(red)[ERIK klarrr men kan nog utökas]
Balancing realism and gameplay was a recurring challenge. The solar systems are not to scale, as realistic distances made planets too far apart to be visible during exploration. To address this, all celestial bodies were scaled down to ensure visibility and a better gameplay experience.

Planetary motion also posed challenges for surface exploration, as moving planets affected player physics. One considered solution was to freeze a planet’s movement when a player was on it, simplifying implementation by removing velocity effects. However, this was rejected in favor of maintaining physical accuracy, therefore, planets continue moving at all times.

=== EXO explorer differences #text(red)[ERIK klar men kan nog utökas]
#text(red)[Behöver man gå mer i detalj kring hur vi skiljer oss? Alltså exakt vilka features de hade och vilka vi inte har osv?]

This project explored similar areas as the previously mentioned Exo Explorer, but with a greater emphasis on an advanced physics engine, proper benchmarking, and real-time exploration of a procedurally generated galaxy. By contrast, Exo Explorer focused on a single solar system, allowing for deeper detail in planetary environments, whereas this thesis prioritizes scalable procedural systems suitable for rendering and simulating large-scale space exploration.

== Process/Method Discussion - #text(red)[METATEXT]
This sub section provides a discussion for our process and method. Discussions surrounding the overall result, usage of multiple programming languages, the chosen workflow, how AI was used, and how some things changed from the planning stage throughout the project are included.

=== Multiple programming languages - #text(red)[JONATAN, ANTON, JACOB klar]
This project utilized a multi-language approach within Godot, integrating C\#, GDScript, and Rust via the GDExtension system @GDExtension. This strategy allowed us to select the optimal language for specific tasks. Due to being officially supported and allowing for fast development iterations, C\# and GDScript was in used for most of the game logic.

For computationally intensive components, the physics engine in particular, Rust was chosen. Its strengths in raw performance, memory safety, and concurrency enabled significant optimization #text(red)[Källa?]. We leveraged Rust's capabilities and the `rayon` crate to parallelize demanding calculations like particle sorting for octree building and the N-body force computations within the Barnes-Hut algorithm.

While this hybrid approach provided substantial performance benefits for critical sections, it introduced complexities. Managing a multi-language build process, debugging across the GDExtension boundary, and passing data between Rust and C\#/GDScript required careful setup and proved to be cumbersome on occasion. However, the overall experience was positive, confirming that leveraging each language's strengths was advantageous for achieving the project's simulation goals despite the added overhead.

=== Workflow and Collaboration #text(red)[Jonatan klar ERIK klar]
#text(red)[Bra skrivet men lite upprepning från planeringskapitlet under rubriken "introduction", tror inte vi behöver prata om hur vi jobbade eller vad vi för arbetssätt igen (eftersom vi redan gjort det förut eller?), utan mer vad vi tyckte om hur vi jobbade.]

#text(red)[
  Förslag på text utan en längre förklaring av hur vi jobbade:

  The group followed an Agile-adjacent workflow (detailed in #ref(<Workflow>)) centered on a GitHub Projects Kanban board for task tracking, with version control managed through Git and a feature-branch model on GitHub (as detailed in #ref(<Git-section>)). GitHub also supported PR reviews, issue tracking, and rule enforcement, while Discord facilitated team communication and meetings.

  While this structured approach was largely effective, maintaining detailed Kanban updates and strict process adherence became more challenging towards the project's end due to increased time pressure from integration and bug fixing. Nevertheless, the core elements—feature branches, enforced PRs, and centralized task tracking—proved essential for managing collaborative development throughout the project.
]

Our team employed an agile-inspired workflow, centered around a GitHub Projects Kanban board for task management (tracking "To Do" through "Done," with defined acceptance criteria and self-assignment by team members for clarity on task ownership). This was complemented by iterative, weekly planning cycles aligned with supervisor meetings, allowing for adaptive prioritization and progress.

Version control and collaboration relied on Git with a feature branch workflow hosted on GitHub. Each feature was developed in isolation, facilitating parallel work. Crucially, GitHub branch protection rules were enforced: direct pushes to the `master` branch were disallowed, and Pull Requests (PRs) required at least one peer review and approval before merging. This mandatory code review process was vital for quality assurance, knowledge sharing, and maintaining an overview of progress.

GitHub served as the central hub for repositories, the Kanban board, PRs, issue tracking, and rule enforcement. Discord was our primary platform for all team communication, including text discussions and digital meetings.

While this structured approach was largely effective, maintaining detailed Kanban updates and strict process adherence became more challenging towards the project's end due to increased time pressure from integration and bug fixing. Nevertheless, the core elements—feature branches, enforced PRs, and centralized task tracking—proved essential for managing collaborative development throughout the project.


=== Use of generative AI - #text(red)[William, Jacob klar]
Artificial intelligence (AI) was utilized at various stages throughout the project. During the development phase, tools such as ChatGPT and GitHub Copilot were employed to support the coding process. Copilot was also integrated into the pull request (PR) review workflow, providing quick feedback on code submissions. While AI-generated reviews were not considered substitutes for peer-reviewed evaluations, they offered an efficient means of identifying obvious issues that might otherwise be overlooked.

Additionally, AI was employed during the report writing phase. Tools such as ChatGPT and Google Gemini were occasionally used to refine written text and enhance the overall quality of the writing.

=== Project purpose #text(red)[Jacob, ERIK klar]
The projects purpose underwent a greater change after the feedback from the planning report. The original purpose was what follows:

#block(
  fill: luma(230),
  inset: 8pt,
  radius: 4pt,
)[
  "The aim of this project is to simulate solar systems through procedurally generated planets, utilizing computer-generated noise such as Perlin noise, together with the marching cubes algorithm. The composition of the solar systems can vary – from a sun with a single planet to more complex systems with multiple planets and additional celestial bodies such as moons. To mimic the natural movements of these celestial bodies, a simplified physics simulation will be implemented.

  This project also aims to explore and combine different techniques for optimization to ensure that the simulation will run in a performance-efficient manner."
]

It became clear that the project’s objectives were not clearly defined. After internal discussions the team reached a consensus on the purpose of the project. Any changes, particularly to the purpose, sought to address the following three problems:

1. A great deal had been explained specifically about solar systems in the planning report, while the team, in reality, had drifted towards wanting to create an entire galaxy of solar systems instead.
2. An entire section of the planning report was dedicated to performance and optimization, as well as a part of the purpose. This played a part in making it unclear whether this projects major focus was about optimization, or something else.
3. It was unclear how this project differs from the similar bachelor's thesis project Exo Explorer@exo_exporer:2023, from a couple of years ago.

Through internal discussions, consultation with our supervisor, and study of the previously mentioned feedback, the purpose was rewritten to clarify and refine the project's goals and scope. The resulting purpose can be seen in @purpose-ref.

=== MoSCoW changes #text(red)[Jacob, ERIK KLAR]
The MoSCoW method was used to structure and prioritize project features into tiers of importance. As the project progressed and it's scope became clearer, the MoSCow table underwent change. Due to the agile workflow of the project, features were re-prioritized, removed or added.

While most features remained unchanged throughout, some of the most notable changes were:

- Camera/player controls: Moved from "Should have" to "Must have" as the focus on being able to explore the planets, systems, and galaxy, was deemed very important.
- Space background: Removed from "Must have", since it was deemed not critical for project's success. Although, a space background was eventually given by itself as the stars were distributed in the galaxy.
- Galaxy traversal: Added as a "Should have" to allow for traversal at different scales. On the solar galaxy level, the solar system level, and on the planet level.
- Other celestial bodies e.g. asteroids or nebulae, as well as planet vegetation, were added as potential features to be added. In the end planet vegetation was explored.



=== Performance #text(red)[Jacob, ERIK klar]
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

Shortly thereafter, a target of maintaining an average of 60 FPS was determined as a concrete benchmark to strive toward. The performance varies between computer systems with different computer hardware, so this target was to be achieved on a specific benchmarking computer with specific hardware specifications, those specifications were detailed in @pc-1-specs.

However, as development progressed and with greater research into real-time performance, the team improved its understanding of what constitutes a smooth and responsive performance. Rather than aiming for a high average FPS, the focus shifted towards consistency in frame times instead, this updated benchmarking methodology is described in @benchmarking-and-performance-ref.

Regarding the other original metrics:

- *Scene generation time:* This refers to how long it takes to initialize and load new scenes and was initially marked as a performance metric. However, since the majority of elements are streamed at runtime, rather than through traditional loading screens in between, the metric was eventually deemed less critical. Even so, it was still utilized as a metric for some operations, such as during planet generation, to compare different implementations and optimizations.

- *Memory consumption:* While initially a concern, it proved not to be a limiting factor in practice. This is likely due to the content being generated procedurally at runtime, rather than pre-loaded or stored in memory. Although it was continiously monitored, no memory-related issues occurred, making it a non-critical performance metric for this project.

== Generalizability and Validity - #text(red)[Jonatan KLAR ]
This section considers the broader applicability of the project's components and the soundness of its simulation results.

=== Generalizability
Many techniques employed in this project are highly generalizable. The implemented N-body algorithms are standard methods applicable to other systems governed by inverse-square laws (like gravity or electrostatics). The Morton-based octree construction represents an efficient approach for spatial partitioning relevant to various particle simulations. Furthermore, optimization strategies like octree-driven Level of Detail (LOD), chunking, multi-mesh instancing, and CPU parallelism (for example using `rayon`) are standard practices widely used across real-time 3D graphics and game development for managing large-scale environments and computations. The procedural generation techniques (noise, Marching Cubes) are also broadly applicable. Finally, the integration with Godot via GDExtension also demonstrates a general pattern for offloading heavy computations from the game engine to a high-performance Rust backend.

=== Validity <validity>
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

The second concern focused on the potential for PCG to produce content that is overly repetitive, thereby decreasing the quality of the user experience. While the primary objective of this project was not to create an engaging game play experience, certain measures were nonetheless taken to reduce repetitiveness. For instance, planetary coloration was randomized based on each planet’s distance from the sun, with additional randomization applied to simulate variations in atmospheric thickness. These methods introduced greater diversity in the generated content, demonstrating that careful parameterization and randomness can effectively counteract some of the inherent risks associated with procedural generation.


== Future work - #text(red)[William klar, Jacob klar - jag gav inte förslag på ny text, kanske något jag kan göra. Men du får gärna kolla på mina kommenterer annars :)]
There are several directions in which this project could be expanded. A number of planned features were not implemented due to time constraints, and these could serve as valuable additions in future iterations.

In particular, the planet generation system offers significant room for enhancement. At present, the generated planets include basic features such as vegetation (e.g., trees and grass) and bodies of water (e.g., oceans), but they remain relatively un-engaging. Future improvements could include the addition of fauna, subterranean structures such as cave systems #text(red)[(Vill man nämna för att "dra mer nytta av marching cubes-implementationen"? Den är väl ändå på plats för att kunna lägga till grottor, osv)], and other biome-specific features to increase diversity and immersion within the planet.

Another potential extension involves incorporating a wider range of celestial bodies #text(red)["och att det finns i vår moscow som 'could have'", men som vi inte hann arbeta på? Bara för att koppla till Moscow:en.], such as gas giants, meteoroids, and comets. This would significantly enhance the diversity and complexity of the planetary generation system. A realistic galaxy comprises various types of astronomical objects, not solely solid-surface planets. Currently, the project does not convey this diversity, and expanding the range of celestial bodies would contribute to a more authentic and immersive galactic environment.

#text(red)[
 Sure, kanske, men det måste inte heller vara så att man vill att spelaren ska påverkas mycket mer än vad den redan gör. Jag tror vad som vore viktigare att nämna är "_Increasing the *utilization of the existing* physics system...blabla_", "man kan se ett hint av det i @physics-galaxy-ref ...blabla".
  
Enhancing the physics system is another area which could be expanded upon. Currently, the player character is not fully integrated into the simulation of celestial bodies; gravitational effects are applied only when the player is in close proximity to a planet, rather than being simulated by the implemented physics engine (see #ref(<physics-engine-ref>) and #ref(<player-controls-ref>)). Integrating the player into the same physics framework as the celestial bodies would increase realism and coherence within the simulation.
]

Overall, the project presents many opportunities for refinement and expansion, particularly in the areas of planetary diversity and physical simulation. Enhancing these aspects would contribute to a more engaging and immersive user experience.



= Conclusion - #text(red)[Jonatan KLAR ISH]
This project set out to develop and simulate a physics-based, procedurally generated, and explorable galaxy within the Godot engine, addressing the inherent challenges of computational scale, performance, and plausibility in creating such vast virtual environments. The core objective was to produce a deterministic and computationally efficient model capable of real-time interaction.

Development resulted in a comprehensive system. Planets were successfully generated using a combination of noise functions (Perlin, fBm) and the Marching Cubes algorithm, resulting in complex and varied terrains. These planets were further enhanced with procedurally placed features, including oceans with shader-based wave effects, vegetation distributed via Poisson-disc sampling, and physically-inspired atmospheres simulating Rayleigh scattering. Crucially, significant performance optimizations were implemented and validated; multi-threading effectively offloaded intensive planet generation tasks, while octree-based chunking with Level of Detail (LOD) dynamically managed mesh complexity, and MultiMesh instancing drastically reduced draw calls for star rendering. A robust N-body physics engine, leveraging a parallelized Barnes-Hut algorithm implemented in Rust, was integrated to simulate celestial mechanics efficiently for numerous bodies. The use of seeded randomization throughout ensured deterministic generation, allowing for reproducible galaxies and solar systems. Exploration capabilities were successfully implemented, allowing navigation across multiple scales via a galaxy map and seamless transitions into solar systems down to planetary surface interaction with a physics-aware controller.

The project successfully met its primary goals, fulfilling the essential "Must Have" and "Should Have" requirements defined during planning. The focus on achieving consistent frame times, rather than solely maximizing average FPS, proved effective in delivering a smoother user experience during exploration and simulation.

Ultimately, this work contributes a practical implementation and analysis of techniques essential for simulating large-scale procedural galaxies. It demonstrates the successful integration of advanced procedural generation, N-body physics simulation, and targeted optimization strategies within the Godot engine, offering a viable and efficient model for developers aiming to create expansive, dynamic, and interactive celestial environments. While acknowledging necessary simplifications for real-time performance, the project establishes a solid foundation upon which future enhancements, such as greater celestial diversity or more complex physical interactions, can be built.

#[


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

=== Galaxy summary
1. The Star Field (@star-field-ref), of randomly distributed stars within a cubic volume with stars of a single mesh. It served as a spatial proof of concept.
2. Next, the Disc Galaxy (@disc-galaxy-ref), shaped to resemble a disc-shaped galaxy. With stars sampled within a sphere, and placed via a distance-based probability, to mimic a disc shape.
3. A Skybox (@skybox-ref), a pre-rendered star field image, mostly used in testing environments.
4. The Infinite Galaxy (@infinite-galaxy-ref), removed spatial boundaries of the Star Field, by generating new chunks of stars in any direction that the user moves.
5. An experimental Physics-Based Galaxy (@physics-galaxy-ref), that simulates real gravitational interactions between stars, within the Disc Galaxy implementation. Although it lacked stability due to no proper mass and initial velocity of each star, it demonstrated the capabilities of the Physics Engine.




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
    If(cond: $s_n^2 / d^2 < theta^2$, {
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

]<no-wc>