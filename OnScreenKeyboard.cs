#region File Description
//-----------------------------------------------------------------------------
// ThunderForge OnScreenKeyboard
// Greg Rozmarynowycz
// http://thunderforge.co	
//
// Based on:
// ---
// OnScreenKeyboard by tophathacker
// 
// https://monogame.codeplex.com/discussions/358999
// https://github.com/tophathacker/XnaOnScreenKeyboard
// ---
// SafeAreaOverlay.cs
//
// Microsoft XNA Community Game Platform
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace ThunderForge.Utilities
{
	enum KeyboardMode
	{
		Default,
		Shift,
		CapsLock
	}

	/// <summary>
	/// Resolution agnostic on-screen touch keyboard for Monogame/XNA
	/// </summary>
	public class OnScreenKeyboard : DrawableGameComponent
	{
		/// <summary>
		/// The display font for the keyboard. Must contain characters &amp;#8678 - &amp;#8691 (arrows), ex. Segoe UI Symbol
		/// </summary>
		public SpriteFont Font;

		TimeSpan lastTouchTime;
		int lastKeyDown;
		List<Keys> keysDown;

		float keyCooldown = .15f;
		KeyboardMode mode = KeyboardMode.Default;

		public Color backgroundColor;

		public Color buttonColor1;
		public Color buttonColor2;
		public Color buttonColor3;

		public Color textColor;
		public Color altTextColor;
		public float altCharge = .7f;
		public float highlightDuration = .33f;

		SpriteBatch spriteBatch;
		Texture2D kbTexture;
		Dictionary<Keys, TouchKey> touchKeys;

		int keyboardWidth;
		int keyboardHeight;
		int keyWidth;
		int keyHeight;
		/// <summary>
		/// Padding of keys as a percentage of the bounding box width
		/// </summary>
		public float keyPadding = .05f;

		/// <summary>
		/// Creates an touch keyboard. If Font load fails, must set font before use
		/// </summary>
		public OnScreenKeyboard(Game a_game)
			: base(a_game)
		{
			keysDown = new List<Keys>();

			// Choose a high number, so we will draw on top of other components.
			DrawOrder = 1000;

			//Set default values for the draw colors
			backgroundColor = new Color(50, 50, 50);

			buttonColor1 = Color.DimGray;
			buttonColor2 = Color.SteelBlue;
			buttonColor3 = Color.DodgerBlue;

			textColor = Color.White;
			textColor = Color.LightGray;

			//Hard-coded definitions for each key on the keyboard
			#region Key Definitions
			touchKeys = new Dictionary<Keys, TouchKey>();

			//First row of keys
			touchKeys.Add(Keys.Q, new TouchKey(0, "q", "Q", "1", Keys.Q, Keys.D1));
			touchKeys.Add(Keys.W, new TouchKey(1, "w", "W", "2", Keys.W, Keys.D2));
			touchKeys.Add(Keys.E, new TouchKey(2, "e", "E", "3", Keys.E, Keys.D3));
			touchKeys.Add(Keys.R, new TouchKey(3, "r", "R", "4", Keys.R, Keys.D4));
			touchKeys.Add(Keys.T, new TouchKey(4, "t", "T", "5", Keys.T, Keys.D5));
			touchKeys.Add(Keys.Y, new TouchKey(5, "y", "Y", "6", Keys.Y, Keys.D6));
			touchKeys.Add(Keys.U, new TouchKey(6, "u", "U", "7", Keys.U, Keys.D7));
			touchKeys.Add(Keys.I, new TouchKey(7, "i", "I", "8", Keys.I, Keys.D8));
			touchKeys.Add(Keys.O, new TouchKey(8, "o", "O", "9", Keys.O, Keys.D9));
			touchKeys.Add(Keys.P, new TouchKey(9, "p", "P", "0", Keys.P, Keys.D0));
			touchKeys.Add(Keys.Back, new TouchKey(10, "⇦Backspace", "⇦Backspace", "", Keys.Back) { width = 2, fontScale = .67f, repeat = true });

			//Row 2
			touchKeys.Add(Keys.A, new TouchKey(11, "a", "A", "", Keys.A) { offset = .5f });
			touchKeys.Add(Keys.S, new TouchKey(12, "s", "S", "", Keys.S));
			touchKeys.Add(Keys.D, new TouchKey(13, "d", "D", "", Keys.D));
			touchKeys.Add(Keys.F, new TouchKey(14, "f", "F", "", Keys.F));
			touchKeys.Add(Keys.G, new TouchKey(15, "g", "G", "", Keys.G));
			touchKeys.Add(Keys.H, new TouchKey(16, "h", "H", "", Keys.H));
			touchKeys.Add(Keys.J, new TouchKey(17, "j", "J", "", Keys.J));
			touchKeys.Add(Keys.K, new TouchKey(18, "k", "K", "", Keys.K));
			touchKeys.Add(Keys.L, new TouchKey(19, "l", "L", "", Keys.L));
			touchKeys.Add(Keys.Enter, new TouchKey(20, "Return", "Return", "", Keys.Enter) { width = 2, fontScale = .67f });

			//Row 3
			touchKeys.Add(Keys.LeftShift, new TouchKey(21, "⇧Shift", "⇧Shift", "", Keys.LeftShift) { fontScale = .67f });
			touchKeys.Add(Keys.Z, new TouchKey(21, "z", "Z", "", Keys.Z));
			touchKeys.Add(Keys.X, new TouchKey(22, "x", "X", "", Keys.X));
			touchKeys.Add(Keys.C, new TouchKey(23, "c", "C", "", Keys.C));
			touchKeys.Add(Keys.V, new TouchKey(24, "v", "V", "", Keys.V));
			touchKeys.Add(Keys.B, new TouchKey(25, "b", "B", "", Keys.B));
			touchKeys.Add(Keys.N, new TouchKey(26, "n", "N", "", Keys.N));
			touchKeys.Add(Keys.M, new TouchKey(27, "m", "M", "", Keys.M));
			touchKeys.Add(Keys.RightShift, new TouchKey(28, "⇧Shift", "⇧Shift", "", Keys.RightShift) { offset = 2.5f, width = 1.5f, fontScale = .67f });

			//Row 4
			touchKeys.Add(Keys.Space, new TouchKey(29, "", "", "", Keys.Space) { offset = 3, width = 5 });
			touchKeys.Add(Keys.Escape, new TouchKey(30, "Close ⇩", "Close ⇩", "", Keys.Escape) { offset = 3, fontScale = .75f });

			#endregion;
		}

		/// <summary>
		/// Returns the state of touch keyboard
		/// </summary>
		/// <returns></returns>
		public KeyboardState GetState()
		{
			return new KeyboardState(keysDown.ToArray());
		}

		/// <summary>
		/// Process input and perform update logic each cycle
		/// </summary>
		/// <param name="gameTime">XNA object containing current game time properties</param>
		public override void Update(GameTime gameTime)
		{
			//empty the keys down list
			keysDown.Clear();

			//time since last update
			float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

			//time since last key touch
			double lastTouchDelta = gameTime.TotalGameTime.TotalSeconds - lastTouchTime.TotalSeconds;

			#region Process Key Data
			//Update the properties of each touch key
			foreach (TouchKey key in touchKeys.Values)
			{
				//Update the elapsed key press time
				if (key.pressed)
				{
					key.pressedElapsed += deltaTime;
				}

				/* Register key touch as input
				 * 
				 * Keys are register under the follwing conditions:
				 *	- The key's alternate action has not been fired
				 *		AND
				 *		- The key is no longer pressed and was pressed for at least one cycle
				 *			OR
				 *		- The key has an alternate action and has been held beyond the altCharge time
				 *		
				 *		OR
				 *	- The key is pressed, it is allowed to repeat without being released, and it does not have an alternate action
				 */
				if ((!key.pressed && key.pressedElapsed > 0 || key.pressedElapsed > altCharge && key.altKey != Keys.None) && !key.altKeyFired || key.pressed && key.repeat && key.altKey == Keys.None)
				{
					// we have to prevent key commands from being generated too fast
					// This is a hacky way to implement this last condition and isn't full proof
					bool registerKey = true;
					if (lastTouchDelta < keyCooldown)
						if (lastKeyDown == key.keyIndex)
							registerKey = false;

					//if a commmand for the same key has not been recently generated (relatively speaking)
					if (registerKey)
					{
						//Register this as the last key touched
						//If another key is register immediately after this, key commands can be generated at high frequency
						lastKeyDown = key.keyIndex;
						lastTouchTime = gameTime.TotalGameTime;

						Keys registeredKey = key.key;
						//Check if alternate key action should be registered instead of the default
						if (key.altKey != Keys.None && key.pressedElapsed > altCharge)
						{
							registeredKey = key.altKey;
							key.altKeyFired = true;
							key.activeColor = buttonColor3;
						}

						//register to the outputted key array
						keysDown.Add(registeredKey);
					}
				}

				//If the key is no longer being pressed, reset its state
				if (!key.pressed)
				{
					key.pressedElapsed = 0;
					key.altKeyFired = false;
				}

				//for the purpose of processing input, we assume the key is longer pressed each cycle
				key.pressed = false;

				//Update remaning key highlight time
				key.highlightRemaining -= deltaTime;
				if (key.highlightRemaining <= 0)
				{
					key.highlightRemaining = 0;
				}
			}
			#endregion

			#region Process Screen Input
			//Collect and process touch input
			TouchCollection collection = TouchPanel.GetState();
			foreach (TouchLocation location in collection)
			{
				processTouch(location.Position, gameTime);
			}

			//Collect and process mouse input
			MouseState mouse = Mouse.GetState();
			if (mouse.LeftButton == ButtonState.Pressed)
				processTouch(new Vector2(mouse.X, mouse.Y), gameTime);
			#endregion

			//Perform keyboard state logic for shift keys
			if (mode == KeyboardMode.Shift || mode == KeyboardMode.CapsLock)
			{
				if (keysDown.Count > 0 && mode == KeyboardMode.Shift && !keysDown.Contains(Keys.LeftShift) && !keysDown.Contains(Keys.RightShift))
				{
					mode = KeyboardMode.Default;
				}

				keysDown.Add(Keys.LeftShift);
			}

			base.Update(gameTime);
		}

		/// <summary>
		/// Takes the position of a touch and determines which key, if any was pressed
		/// </summary>
		/// <param name="position">The position of the touch</param>
		/// <param name="gameTime">XNA object containing current game time properties</param>
		private void processTouch(Vector2 position, GameTime gameTime)
		{
			double lastTouchDelta = gameTime.TotalGameTime.TotalSeconds - lastTouchTime.TotalSeconds;

			if (touchKeys != null)
			{
				foreach (TouchKey key in touchKeys.Values)
				{
					if (key.boundingBox.Contains(position))
					{
						//indicate the key was pressed this cycle
						key.pressed = true;

						//reset the highlight color if this is the initial touch
						if (key.pressedElapsed == 0)
						{
							key.activeColor = buttonColor2;
						}

						//reset the highlight cooldown
						key.highlightRemaining = highlightDuration;

						//Apply shift modes
						if ((key.key == Keys.RightShift || key.key == Keys.LeftShift) && key.pressedElapsed == 0)
						{
							switch (mode)
							{
								case KeyboardMode.Default:
									mode = KeyboardMode.Shift;
									break;
								case KeyboardMode.Shift:
									mode = (lastTouchDelta < .3) ? KeyboardMode.CapsLock : KeyboardMode.Default;
									break;
								case KeyboardMode.CapsLock:
									mode = KeyboardMode.Default;
									break;
							}
						}

						//Check if the keyboard was collapsed
						if (key.key == Keys.Escape)
						{
							Enabled = false;
						}
					}
				}
			}
		}

		public void UpdateKeyboardSize(object o, System.EventArgs e)
		{
			SetKeyboardDimensions();
		}

		/// <summary>
		/// Sets the dimensions of the keyboard components in a resolution agnostic manner
		/// </summary>
		protected void SetKeyboardDimensions()
		{
			//Calculate basic dimensions
			keyboardWidth = Game.GraphicsDevice.Viewport.Width;
			keyboardHeight = Game.GraphicsDevice.Viewport.Height * 40 / 100;
			keyWidth = keyboardWidth / 12;
			keyHeight = keyboardHeight / 4;

			//The starting point to draw the keys from
			Vector2 pointer = new Vector2(0, GraphicsDevice.Viewport.Height - keyboardHeight);

			//Loop through each touch key and draw them across the screen
			foreach (TouchKey key in touchKeys.Values)
			{
				//The active area of the key
				key.boundingBox = new Rectangle((int)(pointer.X + key.offset * keyWidth), (int)pointer.Y, (int)(keyWidth * key.width), keyHeight);

				//calculate the dimensions and position of the visual representation of the key
				int paddingThickness = (int)(keyWidth * keyPadding);

				//apply padding to the key
				int dBoxX = (int)(key.boundingBox.X + paddingThickness);
				int dBoxY = (int)(key.boundingBox.Y + paddingThickness);
				int dBoxWidth = (int)(key.boundingBox.Width - 2 * paddingThickness);
				int dBoxHeight = (int)(key.boundingBox.Height - 2 * paddingThickness);

				key.drawBox = new Rectangle(dBoxX, dBoxY, dBoxWidth, dBoxHeight);

				//Advance the position of the draw pointer
				pointer += new Vector2(key.boundingBox.Width + key.offset * keyWidth, 0);
				if (pointer.X + keyWidth > keyboardWidth)
				{
					pointer += new Vector2(-pointer.X, keyHeight);
				}
			}
		}

		/// <summary>
		/// Initialize the keyboard
		/// </summary>
		public override void Initialize()
		{
			base.Initialize();
			this.Game.GraphicsDevice.DeviceReset += new System.EventHandler<System.EventArgs>(UpdateKeyboardSize);
			this.SetKeyboardDimensions();
		}

		/// <summary>
		/// Creates the graphics resources needed to draw the overlay.
		/// </summary>
		protected override void LoadContent()
		{
			spriteBatch = new SpriteBatch(GraphicsDevice);

			// Create a 1x1 white texture.
			kbTexture = new Texture2D(GraphicsDevice, 1, 1);
			kbTexture.SetData(new Color[] { Color.White });

			//Attempt to load the font from the root content directory
			try
			{
				Font = Game.Content.Load<SpriteFont>("Segoe UI Symbol");
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Unable to load OnScreenKeyboard Font: " + ex.Message);
			}
			
		}


		/// <summary>
		/// Draws the keyboard
		/// </summary>
		public override void Draw(GameTime gameTime)
		{
			if (!Enabled)
				return;

			spriteBatch.Begin();

			// Compute four border rectangles around the edges of the safe area.
			Rectangle keyboardDrawBox = new Rectangle(0, GraphicsDevice.Viewport.Height - keyboardHeight, keyboardWidth, keyboardHeight);
			spriteBatch.Draw(kbTexture, keyboardDrawBox, backgroundColor);

			//Calculate dimensions and visual properties for keys, draw them
			foreach (TouchKey key in touchKeys.Values)
			{
				#region Draw Key Box
				//start with default key background color
				Color keyBg = buttonColor1;

				//Apply highlight color and fade behavior to touched keys
				if (key.highlightRemaining > 0)
				{
					float colorCoord = (float)(key.highlightRemaining / highlightDuration);
					int R = (int)(buttonColor1.R + (key.activeColor.R - buttonColor1.R) * colorCoord);
					int G = (int)(buttonColor1.G + (key.activeColor.G - buttonColor1.G) * colorCoord);
					int B = (int)(buttonColor1.B + (key.activeColor.B - buttonColor1.B) * colorCoord);

					keyBg = new Color(R, G, B);
				}

				//Apply special coloring rules to shift keys
				if (key.key == Keys.LeftShift || key.key == Keys.RightShift)
				{
					switch (mode)
					{
						case KeyboardMode.Shift:
							keyBg = buttonColor2;
							break;
						case KeyboardMode.CapsLock:
							keyBg = buttonColor3;
							break;
					}
				}

				//Draw the background of the key
				spriteBatch.Draw(kbTexture, key.drawBox, keyBg);
				#endregion

				#region Draw Key Text
				//determine what text to display on the key
				string displayString = (mode == KeyboardMode.Shift || mode == KeyboardMode.CapsLock) ? key.displayUpper : key.displayLower;

				float fontSize = .5f;
				float fontScale = fontSize / ((float)Font.LineSpacing / (float)keyHeight) * key.fontScale;
				Vector2 textPosition = new Vector2(key.drawBox.X + key.drawBox.Width * 1 / 10, key.drawBox.Y + key.drawBox.Height * 2 / 10);

				spriteBatch.DrawString(Font, displayString, textPosition, textColor, 0, Vector2.Zero, fontScale, SpriteEffects.None, 0);

				//if the key has an alternate key action, draw its corresponding text
				if (key.displayAlt != String.Empty)
				{
					fontSize = .25f;
					fontScale = fontSize / ((float)Font.LineSpacing / (float)keyHeight) * key.fontScale;
					textPosition = new Vector2(key.drawBox.X + key.drawBox.Width * 8 / 10, key.drawBox.Y + key.drawBox.Height * 5 / 100);

					spriteBatch.DrawString(Font, key.displayAlt, textPosition, textColor, 0, Vector2.Zero, fontScale, SpriteEffects.None, 0);
				}
				#endregion
			}
			spriteBatch.End();
		}
	}
}