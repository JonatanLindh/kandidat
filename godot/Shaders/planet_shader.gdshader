shader_type spatial;

uniform float min_height;
uniform float max_height;
uniform sampler2D height_color;

varying float height;

void vertex() {
	height = length(VERTEX);
}

void fragment() {
	float t = (height / (max_height-min_height)) - (min_height / (max_height-min_height));
	t += 0.01;
	vec3 color = texture(height_color, vec2(t, 0.)).rgb;
	
	ALBEDO = color;
}

//void light() {
	// Called for every pixel for every light affecting the material.
	// Uncomment to replace the default light processing function with this one.
//}
