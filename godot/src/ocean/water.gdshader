shader_type spatial;

uniform vec3 color : source_color;
uniform vec3 highlight_color : source_color;
uniform float metal : hint_range(0.0, 1.0, 0.01);
uniform float roughness : hint_range(0.0, 1.0, 0.01);
uniform float specular : hint_range(0.0, 1.0, 0.01);

uniform sampler2D bump_map1;
uniform sampler2D bump_map2;
uniform sampler3D texture_3d;

uniform samplerCube cubemap;
uniform vec2 dir_bump_map1 = vec2(1,0);
uniform vec2 dir_bump_map2 = vec2(0,1);
uniform float speed_bump_map1 = 0.01;
uniform float speed_bump_map2 = 0.01;

uniform sampler2D depth_texture : hint_depth_texture;

void vertex() {
	// Called for every vertex the material is visible on.
}

void fragment() {
	// Called for every pixel the material is visible on.
	
	// From docs https://docs.godotengine.org/en/stable/tutorials/shaders/using_viewport_as_texture.html#making-the-planet-texture
	// Converts UV coordinates to cartesian coordinates to avoid pinching at the poles
	float theta = UV.y * 3.14159;
	float phi = UV.x * 3.14159 * 2.0;
	vec3 unit = vec3(0.0, 0.0, 0.0);

	unit.x = sin(phi) * sin(theta);
	unit.y = cos(theta) * -1.0;
	unit.z = cos(phi) * sin(theta);
	unit = normalize(unit);
	
	// use new uv coordinates to sample from 3d noise texture
	vec3 bump11_color = texture(bump_map1, UV + (TIME * dir_bump_map1 * speed_bump_map1)).rgb;
	vec3 bump22_color = texture(bump_map2, UV + (TIME * dir_bump_map2 * speed_bump_map2)).rgb;
	vec3 interpolated_bump12_color = mix(bump11_color, bump22_color, 0.5);
	
	vec3 bump1_color = texture(texture_3d, unit.xyz + (TIME * vec3(dir_bump_map1,1) * speed_bump_map1)).rgb;
	vec3 bump2_color = texture(texture_3d, unit.xyz + (TIME * vec3(dir_bump_map2,1) * speed_bump_map2)).rgb;
	vec3 interpolated_bump_color = mix(bump1_color, bump2_color, 0.5);
	
	vec3 water_surface = mix(interpolated_bump_color, interpolated_bump12_color, 0.8);
	NORMAL_MAP = water_surface;
	
	vec3 distance_to_center = VERTEX;
	float depth = texture(depth_texture, SCREEN_UV).x;
	
	/*
 	float depth = texture(depth_texture, SCREEN_UV).x;
 	vec3 ndc = vec3(SCREEN_UV * 2.0 - 1.0, depth);
  	vec4 view = INV_PROJECTION_MATRIX * vec4(ndc, 1.0);
  	view.xyz /= view.w;
  	float linear_depth = -view.z;

  	vec4 world = INV_VIEW_MATRIX * INV_PROJECTION_MATRIX * vec4(ndc, 1.0);
  	vec3 world_position = world.xyz / world.w;*/

	float length_surface_to_water = max(length(distance_to_center), depth) - min(length(distance_to_center), depth);
	vec3 water_color = mix(highlight_color, color, length_surface_to_water / length(distance_to_center));
	ALBEDO = water_color;
	ALPHA = bump1_color.r;
	METALLIC = metal;
	ROUGHNESS = roughness;
	SPECULAR = specular;
	
	
}

//void light() {
//	// Called for every pixel for every light affecting the material.
//	// Uncomment to replace the default light processing function with this one.
//}
