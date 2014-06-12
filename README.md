OnScreenKeyboard
================

Monogame On-Screen Touch Keyboard

ThunderForge
thunderforge.co
Greg Rozmarynowycz

USAGE:
================
// include using statement
using Thunderforge.Utilities;

...
// Instantiate the keyboard:

OnScreenKeyboard touchKeyboard = new OnScreenKeyboard(this);
Components.Add(touchKeyboard);

...
// Then use pretty much just like the standard XNA keyboard

KeyboardState state = touchKeyboard.GetState();

Note: The keyboard will attempt to load the included "Segoe UI Symbols" xnb from the root content 
directory if it fails, the Font field MUST be set before the keyboard can be used. If you wish to use
another font, it must contain characters &amp;#8678 - &amp;#8691 (arrows)
