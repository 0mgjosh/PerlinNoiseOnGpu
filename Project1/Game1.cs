using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Runtime.InteropServices;

namespace Project1
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private Vector2 _screen = new(1280, 1280);
        private SpriteFont _font;
        private Effect _effect;
        private Texture2D _pixel;
        private RenderTarget2D _renderTarget;
        private Texture2D pic;

        private float spin = 0;
        private float scale = 1;
        private Vector2 offset = Vector2.Zero;
        float level = 1;

        float prev_scale;
        Vector2 prev_offset;
        float prev_spin;

        private float perlinAtMouse;

        private Color[] data = new Color[1280 * 1280];

        private float oct;
        float scale_max = 5;
        float scale_min = 0.005f;
        float scale_inc = 0.1f;
        float scale_move_speed = 0.001f;
        float scale_move_speed_mult = 0;
        int max_octaves = 10;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            _graphics.PreferredBackBufferWidth = (int)_screen.X;
            _graphics.PreferredBackBufferHeight = (int)_screen.Y;
            _graphics.ApplyChanges();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _font = Content.Load<SpriteFont>("font");
            _effect = Content.Load<Effect>("effect");
            _pixel = new Texture2D(GraphicsDevice, 1, 1);
            _pixel.SetData([Color.White]);
            _renderTarget = new RenderTarget2D(GraphicsDevice, (int)_screen.X, (int)_screen.Y);
            pic = Content.Load<Texture2D>("Palette");
            //_effect.Parameters["palette"].SetValue(pic);
            SetEffectParameters();
            SetData();
        }

        float previous_Scroll_Value;
        Vector2 mp;
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            KeyboardState kstate = Keyboard.GetState();
            MouseState mstate = Mouse.GetState();
            mp = mstate.Position.ToVector2();

            prev_scale = scale;
            prev_offset = offset;
            prev_spin = spin;

            if(kstate.IsKeyDown(Keys.LeftShift))
            {
                scale_move_speed_mult = 20f;
            } else scale_move_speed_mult = 1f;


            if (kstate.IsKeyDown(Keys.Left)) spin -= 0.1f;
            if (kstate.IsKeyDown(Keys.Right)) spin += 0.1f;


            if (mstate.ScrollWheelValue > previous_Scroll_Value)
            {
                scale -= scale_inc;
                scale = Math.Clamp(scale, scale_min, scale_max);
                previous_Scroll_Value = mstate.ScrollWheelValue;
            }
            if (mstate.ScrollWheelValue < previous_Scroll_Value)
            {
                scale += scale_inc;
                scale = Math.Clamp(scale, scale_min, scale_max);
                previous_Scroll_Value = mstate.ScrollWheelValue;
            }


            if(kstate.IsKeyDown(Keys.Up)) level += 0.01f;
            if(kstate.IsKeyDown(Keys.Down)) level -= 0.01f;


            if (kstate.IsKeyDown(Keys.W)) offset.Y -= scale_move_speed * scale_move_speed_mult;
            if (kstate.IsKeyDown(Keys.S)) offset.Y += scale_move_speed * scale_move_speed_mult;
            if (kstate.IsKeyDown(Keys.A)) offset.X -= scale_move_speed * scale_move_speed_mult;
            if (kstate.IsKeyDown(Keys.D)) offset.X += scale_move_speed * scale_move_speed_mult;

            if(spin != prev_spin || scale != prev_scale || offset != prev_offset)
            {
                SetEffectParameters();
                SetData();
            }

            perlinAtMouse = GetPerlinAt(mp);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            // Draw To Target
            GraphicsDevice.SetRenderTarget(_renderTarget);
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp, sortMode: SpriteSortMode.Immediate);
            _effect.CurrentTechnique.Passes[0].Apply();
            _spriteBatch.Draw(_pixel, new Rectangle(0, 0, (int)_screen.X, (int)_screen.Y), Color.White);
            _spriteBatch.End();
            GraphicsDevice.SetRenderTarget(null);

            // Final Draw
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp, sortMode: SpriteSortMode.Immediate);
            
            _spriteBatch.Draw(_renderTarget, new Rectangle(0, 0, (int)_screen.X, (int)_screen.Y), Color.White);

            _spriteBatch.DrawString(_font, "Spin: " + spin, new Vector2(10, 10), Color.Red);
            _spriteBatch.DrawString(_font, "Scale: " + scale, new Vector2(10, 40), Color.Red);
            _spriteBatch.DrawString(_font, "Offset: " + offset, new Vector2(10, 70), Color.Red);
            _spriteBatch.DrawString(_font, "Perlin ( At Mouse ): " + perlinAtMouse, new Vector2(10, 100), Color.Red);
            _spriteBatch.DrawString(_font, "Mouse Position: " + mp, new Vector2(10, 130), Color.Red);
            _spriteBatch.DrawString(_font, "Level: " + level, new Vector2(10, 160), Color.Red);
            _spriteBatch.DrawString(_font, "Octaves: " + oct, new Vector2(10, 190), Color.Red);

            _spriteBatch.End();
            base.Draw(gameTime);
        }

        int width;
        int index;
        float result;
        Vector2 p;
        float GetPerlinAt(Vector2 position)
        {
            p = position;
            width = _renderTarget.Width;
            index = (int)(p.Y * width + p.X);

            index = Math.Clamp(index, 0, data.Length - 1);

            result = data[index].R;

            return result;
        }

        private void SetData()
        {
            _renderTarget.GetData(data);
        }

        private void SetEffectParameters()
        {
            _effect.Parameters["spin"].SetValue(spin);
            _effect.Parameters["scale"].SetValue(scale);
            _effect.Parameters["offset"].SetValue(offset);
            
            oct = (max_octaves+1)-(max_octaves * (scale/scale_max));
            _effect.Parameters["octave"].SetValue((int)Math.Round(oct+2));
        }
    }
}
