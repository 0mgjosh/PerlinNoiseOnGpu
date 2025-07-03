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
        private Effect _perlin;
        private Effect _colorize;
        private Texture2D _pixel;
        private Texture2D _crosshair;
        private RenderTarget2D _renderTarget;
        private RenderTarget2D _finalTarget;
        private Texture2D Palette;
        private Texture2D noisey;

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
        float scale_max = 20;
        float scale_min = 0.005f;
        float scale_inc = 0.5f;
        float scale_move_speed = 0.1f;
        float move_multiplier = 0;
        int max_octaves = 5;

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
            _perlin = Content.Load<Effect>("effect");
            _colorize = Content.Load<Effect>("Colorize");
            _crosshair = Content.Load<Texture2D>("Crosshair");
            _pixel = new Texture2D(GraphicsDevice, 1, 1);
            _pixel.SetData([Color.White]);
            _renderTarget = new RenderTarget2D(GraphicsDevice, (int)_screen.X, (int)_screen.Y);
            _finalTarget = new RenderTarget2D(GraphicsDevice, (int)_screen.X, (int)_screen.Y);
            Palette = Content.Load<Texture2D>("Palette2");
            noisey = Content.Load<Texture2D>("1280noise");

            _colorize.Parameters["NoiseTexture"].SetValue(noisey);
            SetEffectParameters();
            SetSeeds(33.2353f, 61.3612f);
            //SetColorize();
            SetData();
        }

        float previous_Scroll_Value;
        Vector2 mp;
        float time;
        bool clicked;
        Vector3 sunPos = new();
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            KeyboardState kstate = Keyboard.GetState();
            MouseState mstate = Mouse.GetState();
            Vector2 mPos = mstate.Position.ToVector2();
            mp = mstate.Position.ToVector2();
            time += (float)gameTime.ElapsedGameTime.TotalSeconds;

            sunPos = new Vector3((mPos.X/_screen.X)-.5f,(mPos.Y/_screen.Y)-.5f,.5f);
            //sunPos = new Vector3(0, 0, -1f);
            _perlin.Parameters["SUN"].SetValue(new Vector3(sunPos.X,sunPos.Y,sunPos.Z));

            prev_scale = scale;
            prev_offset = offset;
            prev_spin = spin;

            if(kstate.IsKeyDown(Keys.LeftShift))
            {
                move_multiplier = 1000f;
            } else move_multiplier = 1f;


            if (kstate.IsKeyDown(Keys.Left)) spin -= 0.1f;
            if (kstate.IsKeyDown(Keys.Right)) spin += 0.1f;


            if (mstate.ScrollWheelValue > previous_Scroll_Value)
            {
                scale -= scale_inc*(move_multiplier/5);
                scale = Math.Clamp(scale, scale_min, scale_max);
                previous_Scroll_Value = mstate.ScrollWheelValue;
            }
            if (mstate.ScrollWheelValue < previous_Scroll_Value)
            {
                scale += scale_inc*(move_multiplier / 5);
                scale = Math.Clamp(scale, scale_min, scale_max);
                previous_Scroll_Value = mstate.ScrollWheelValue;
            }


            if(kstate.IsKeyDown(Keys.Up)) level += 0.01f;
            if(kstate.IsKeyDown(Keys.Down)) level -= 0.01f;


            if (kstate.IsKeyDown(Keys.W)) offset.Y -= scale_move_speed * move_multiplier;
            if (kstate.IsKeyDown(Keys.S)) offset.Y += scale_move_speed * move_multiplier;
            if (kstate.IsKeyDown(Keys.A)) offset.X -= scale_move_speed * move_multiplier;
            if (kstate.IsKeyDown(Keys.D)) offset.X += scale_move_speed * move_multiplier;

            if (clicked == false && mstate.LeftButton == ButtonState.Pressed)
            {                
                offset += ((mp/1280)-new Vector2(.5f,.5f))*scale;
                clicked = true;
            }
            else if (clicked == true && mstate.LeftButton == ButtonState.Released)
            {
                clicked = false;
            }

            if (spin != prev_spin || scale != prev_scale || offset != prev_offset)
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
            _perlin.CurrentTechnique.Passes[0].Apply();
            _spriteBatch.Draw(_pixel, new Rectangle(0, 0, (int)_screen.X, (int)_screen.Y), Color.White);
            _spriteBatch.End();
            GraphicsDevice.SetRenderTarget(_finalTarget);

            _spriteBatch.Begin(samplerState: SamplerState.PointClamp, sortMode: SpriteSortMode.Immediate);

            _spriteBatch.Draw(_renderTarget, new Rectangle(0,0, (int)_screen.X, (int)_screen.Y), Color.White);

            _spriteBatch.End();

            GraphicsDevice.SetRenderTarget(null);

            // Draw Final to target
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp, sortMode: SpriteSortMode.Immediate);
            
            _colorize.CurrentTechnique.Passes[0].Apply();

            _spriteBatch.Draw(_finalTarget, new Rectangle(0, 0, (int)_screen.X, (int)_screen.Y), Color.White);

            _spriteBatch.End();

            // Draw Text and crosshair
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp, sortMode: SpriteSortMode.Immediate);

            _spriteBatch.DrawString(_font, "Spin: " + spin, new Vector2(10, 10), Color.Red);
            _spriteBatch.DrawString(_font, "Scale: " + scale, new Vector2(10, 40), Color.Red);
            _spriteBatch.DrawString(_font, "Offset: " + offset, new Vector2(10, 70), Color.Red);
            _spriteBatch.DrawString(_font, "Perlin ( At Mouse ): " + perlinAtMouse, new Vector2(10, 100), Color.Red);
            _spriteBatch.DrawString(_font, "Mouse Position: " + mp, new Vector2(10, 130), Color.Red);
            _spriteBatch.DrawString(_font, "Level: " + level, new Vector2(10, 160), Color.Red);
            _spriteBatch.DrawString(_font, "Octaves: " + oct, new Vector2(10, 190), Color.Red);

            _spriteBatch.Draw(_crosshair, (_screen / 2), null, Color.White, default, new(_crosshair.Width / 2, _crosshair.Height / 2), 3, SpriteEffects.None, default);

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
            _perlin.Parameters["spin"].SetValue(spin);
            _perlin.Parameters["scale"].SetValue(scale);
            _perlin.Parameters["offset"].SetValue(offset);

            oct = (max_octaves+1)-(max_octaves * (scale/scale_max));
            _perlin.Parameters["octave"].SetValue((int)Math.Round(oct+2));
        }

        private void SetSeeds(float worldseed, float tempseed)
        {
            _perlin.Parameters["WORLDSEED"].SetValue(worldseed);
            _perlin.Parameters["TEMPERATURESEED"].SetValue(tempseed);
        }

        private void SetColorize()
        {
            _colorize.Parameters["PaletteTexture"].SetValue(Palette);
        }
    }
}
