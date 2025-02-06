shader_type spatial;

void fragment() {
    vec3 yellow = vec3(1.0, 1.0, 0.0);
    ALBEDO = yellow;
    EMISSION = yellow * 3.0;
}