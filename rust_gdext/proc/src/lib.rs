use proc_macro::TokenStream;
use quote::quote;
use syn::{Ident, ItemFn, parse_macro_input};

/// A procedural macro that controls whether a function should run only in the Godot editor or be denied execution in the editor.
///
/// This attribute macro modifies the annotated function to include a runtime check for whether
/// the code is running in the Godot editor environment.
///
/// # Arguments
///
/// * `only` - Makes the function execute only when in the Godot editor.
/// * `deny` - Makes the function not execute when in the editor.
///
/// # Examples
///
/// ```
/// #[editor(only)]
/// fn setup_editor_tools() {
///     // This code will only run when in the Godot editor
///     // ...
/// }
/// ```
///
/// # Errors
///
/// Compilation will fail if:
/// - No argument is provided: "Missing argument. Use #[editor(only)] or #[editor(deny)]"
/// - An invalid argument is provided: "Invalid argument. Use #[editor(only)] or #[editor(deny)]"
#[proc_macro_attribute]
pub fn editor(args: TokenStream, item: TokenStream) -> TokenStream {
    let mut input = parse_macro_input!(item as ItemFn);

    if args.is_empty() {
        return quote! {
            // We use compile_error! macro to point at the attribute usage site
            #[allow(unused)]
            #input
            compile_error!("Missing argument. Use #[editor(only)] or #[editor(deny)]");
        }
        .into();
    }

    let arg = parse_macro_input!(args as Ident);

    let deny = match arg.to_string().as_str() {
        "deny" => true,
        "only" => false,
        _ => {
            return quote! {
                // We add the attribute with error to the output
                #[allow(unused)]
                #input
                compile_error!("Invalid argument. Use #[editor(only)] or #[editor(deny)]");
            }
            .into();
        }
    };

    let original_body = &input.block.stmts;

    let negation = if deny {
        quote! {}
    } else {
        quote! {!}
    };

    let new_block = quote! {
        {
            if #negation godot::classes::Engine::singleton().is_editor_hint() {
                return;
            }

            #(#original_body)*
        }
    };

    input.block = syn::parse2(new_block).expect("Failed to parse generated code");

    quote! { #input }.into()
}
