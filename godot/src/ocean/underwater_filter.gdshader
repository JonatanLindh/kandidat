shader_type spatial;
render_mode unshaded, blend_mix;

uniform vec3 underwater_tint_color : source_color; 
uniform float underwater_tint_strength : hint_range(0.0, 1.0, 0.01) = 0.5;

uniform sampler2D screen_texture: hint_screen_texture;
varying vec4 origin_world;
varying vec3 camera_position_world;

void vertex() {
	// origin position in world space
	origin_world = MODEL_MATRIX * vec4(0.0, 0.0, 0.0, 1.0);
	camera_position_world = CAMERA_POSITION_WORLD;
	
}

void fragment() {
	// https://docs.godotengine.org/en/stable/tutorials/shaders/screen-reading_shaders.html
	vec3 screen_color = textureLod(screen_texture, SCREEN_UV, 0.0).rgb;
	
	// mix the color from the screen with the underwater color to create the underwater filter
	ALBEDO = mix(screen_color, underwater_tint_color, underwater_tint_strength);
}

