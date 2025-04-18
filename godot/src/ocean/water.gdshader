shader_type spatial;

uniform vec3 deep_color : source_color = vec3(0.0, 91.0, 130.0);
uniform vec3 shallow_color : source_color = vec3(0.0, 142.0, 200.0);
uniform float shallow_size : hint_range(0.0, 16.0, 0.01) = 10.5;
uniform vec3 foam_color : source_color = vec3(0.0, 0.0, 0.0);
uniform float foam_size : hint_range(0.0, 16.0, 0.01) = 0.5;
uniform float transparency_blend : hint_range(0.0, 1.0, 0.01) = 0.5;

uniform float metal : hint_range(0.0, 1.0, 0.01) = 0.0;
uniform float roughness : hint_range(0.0, 1.0, 0.01) = 0.25;
uniform float specular : hint_range(0.0, 1.0, 0.01) = 0.25;

uniform sampler2D bump_map1;
uniform sampler2D bump_map2;
uniform sampler3D texture_3d;
uniform float tex3d_bump_map_blend : hint_range(0.0, 1.0, 0.01) = 0.8;

uniform vec2 dir_bump_map1 = vec2(1,0);
uniform vec2 dir_bump_map2 = vec2(0.25, 0.75);
uniform float speed_bump_map1 = 0.005;
uniform float speed_bump_map2 = 0.005;
uniform float wave_speed = 0.001;

uniform sampler2D depth_texture : hint_depth_texture;
uniform sampler2D screen_texture: hint_screen_texture;

uniform float time;
uniform float delta_time;

varying vec3 world_position;

void vertex() {
	world_position = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xyz;
	float height = texture(bump_map1, world_position.xz + time * wave_speed).r;
//	float wave = sin(world_position.x + TIME * 0.01) * 1.0;
	
	VERTEX += NORMAL * height;
	world_position = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xyz;
}

void fragment() {
	/* -------------------------------- Water Color -------------------------------- */
	
	// The distance from the current camera to the mesh, between [0, 1]
	// where 0 is the near-plane and 1 is the far-plane
 	float depth = texture(depth_texture, SCREEN_UV).r;
	
	// https://docs.godotengine.org/en/latest/tutorials/shaders/advanced_postprocessing.html
	// depth is non-linear so need to make it linear:
	
	// Vulkan (Forward+) uses [-1, 1] in xy but [0, 1] in z. 
	// OpenGL (Compatability) uses [-1, 1] in xyz.
	#if CURRENT_RENDERER == RENDERER_COMPATIBILITY
		vec3 unit_cube_space = vec3(SCREEN_UV, depth) * 2.0 - 1.0;
	#else
		vec3 unit_cube_space = vec3(SCREEN_UV * 2.0 - 1.0, depth);
	#endif
	
  	vec4 view_space = INV_PROJECTION_MATRIX * vec4(unit_cube_space, 1.0);
  	view_space.xyz /= view_space.w; // perspective division - go from homogeneous coordinates
  	//float linear_depth = -view_space.z;
	
	// smooth blending of shallow and deep colors and foam color
	// view_space is relative to the camera so z-coordinate is distance from camera to the mesh
	float smooth_shallow_depth = smoothstep(view_space.z + shallow_size, view_space.z, VERTEX.z);
	float smooth_foam_depth = smoothstep(view_space.z + foam_size, view_space.z, VERTEX.z);
	// TODO Beers law for transition between shallow/deep? (transition is very short now)
	
	// https://docs.godotengine.org/en/stable/tutorials/shaders/screen-reading_shaders.html
	// use screen-texture for water color transparency
	vec3 screen = textureLod(screen_texture, SCREEN_UV, 0.0).rgb;

  	vec4 world = INV_VIEW_MATRIX * INV_PROJECTION_MATRIX * vec4(unit_cube_space, 1.0);
  	vec3 pixel_world_position = world.xyz / world.w;
	//vec3 pixel_world_position = (INV_VIEW_MATRIX * vec4(view_space.xyz, 1.0)).xyz;
	
	// Creates the final water color by blending between deep and shallow colors
	// and then using the screen texture to add transparency and finaly mixing in the foam color
	vec3 water_depth_color = mix(deep_color, shallow_color, smooth_shallow_depth);
	vec3 transparent_water_depth_color = mix(screen * smooth_shallow_depth, water_depth_color, transparency_blend);
	vec3 transparent_water_depth_color_with_foam = mix(transparent_water_depth_color, foam_color, smooth_foam_depth);
	
	ALBEDO = transparent_water_depth_color_with_foam;
	
	/* -------------------------------- Textures and bump-maps -------------------------------- */
	
	// TODO fix pinching at poles.. triplanar uv mapping, cube-sphere..
	// Sample from bump-maps and use as normal map for water surface
	vec3 bump1_color = texture(bump_map1, UV / 5.0 + (time * dir_bump_map1 * speed_bump_map1)).rgb;
	vec3 bump2_color = texture(bump_map2, UV / 5.0 + (time * dir_bump_map2 * speed_bump_map2)).rgb;
	vec3 interpolated_bump_color = mix(bump1_color, bump2_color, 0.5);
	
	// From docs https://docs.godotengine.org/en/stable/tutorials/shaders/using_viewport_as_texture.html#making-the-planet-texture
	// Converts UV coordinates to cartesian coordinates to avoid pinching at the poles with 3D texture
	float theta = UV.y * 3.14159;
	float phi = UV.x * 3.14159 * 2.0;
	vec3 unit = vec3(0.0, 0.0, 0.0);
	unit.x = sin(phi) * sin(theta);
	unit.y = cos(theta) * -1.0;
	unit.z = cos(phi) * sin(theta);
	unit = normalize(unit);
	
	// Use new cartesian UV from above to sample 3d texture
	// Can make it "blotchy"/"spotty" - but I think it looks good to mix in a little bit
	// Try replacing water_surface with interpolated_3d_texture_color as the NORMAL_MAP to see what it looks like
	vec3 texture_3d_color1 = texture(texture_3d, unit.xyz + (time * vec3(dir_bump_map1,1) * speed_bump_map1)).rgb;
	vec3 texture_3d_color2 = texture(texture_3d, unit.xyz + (time * vec3(dir_bump_map2,1) * speed_bump_map2)).rgb;
	vec3 interpolated_3d_texture_color = mix(texture_3d_color1, texture_3d_color2, 0.5);
	
	vec3 water_surface = mix(interpolated_3d_texture_color, interpolated_bump_color, tex3d_bump_map_blend);
	
	NORMAL_MAP = water_surface;

	//ALPHA = smooth_shallow_depth;
	METALLIC = metal;
	ROUGHNESS = roughness;
	SPECULAR = specular;
	
}

//void light() {
//	// Called for every pixel for every light affecting the material.
//	// Uncomment to replace the default light processing function with this one.
//}
