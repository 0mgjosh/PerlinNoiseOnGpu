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

        private float spin = 0;
        private float scale = 1;
        private Vector2 offset = Vector2.Zero;
        private float perlin = 0;

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
        }

        float previous_Scroll_Value;
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            KeyboardState kstate = Keyboard.GetState();
            MouseState mstate = Mouse.GetState();

            if(kstate.IsKeyDown(Keys.Left)) spin -= 0.1f;
            if(kstate.IsKeyDown(Keys.Right)) spin += 0.1f;
            if (mstate.ScrollWheelValue > previous_Scroll_Value)
            {
                scale += 0.5f;
                previous_Scroll_Value = mstate.ScrollWheelValue;
            }
            if (mstate.ScrollWheelValue < previous_Scroll_Value)
            {
                scale -= 0.5f;
                previous_Scroll_Value = mstate.ScrollWheelValue;
            }
            if(kstate.IsKeyDown(Keys.W)) offset.Y -= 1f;
            if(kstate.IsKeyDown(Keys.S)) offset.Y += 1f;
            if(kstate.IsKeyDown(Keys.A)) offset.X -= 1f;
            if(kstate.IsKeyDown(Keys.D)) offset.X += 1f;

            _effect.Parameters["spin"].SetValue(spin);
            _effect.Parameters["scale"].SetValue(scale);
            _effect.Parameters["offset"].SetValue(offset);

            perlin = PerlinNoise(new Vector2(mstate.X, mstate.Y));

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
            _spriteBatch.DrawString(_font, "Perlin ( At Mouse ): " + perlin, new Vector2(10, 100), Color.Red);

            _spriteBatch.End();
            base.Draw(gameTime);
        }

        float PerlinNoise(Vector2 p)
        {
            Vector2 uv = p/_screen;

            uv *= scale;

            Vector2 gridID = Vector2.Floor(uv);
            Vector2 gridUV = Frac(uv);

            Vector2 bl = gridID;
            Vector2 br = gridID + new Vector2(1, 0);
            Vector2 tl = gridID + new Vector2(0, 1);
            Vector2 tr = gridID + new Vector2(1, 1);

            Vector2 gradBl = randomGradient(bl);
            Vector2 gradBr = randomGradient(br);
            Vector2 gradTl = randomGradient(tl);
            Vector2 gradTr = randomGradient(tr);

            Vector2 distFromPixelToBl = gridUV;
            Vector2 distFromPixelToBr = gridUV - new Vector2(1, 0);
            Vector2 distFromPixelToTl = gridUV - new Vector2(0, 1);
            Vector2 distFromPixelToTr = gridUV - new Vector2(1, 1);

            float dotBl = Vector2.Dot(gradBl, distFromPixelToBl);
            float dotBr = Vector2.Dot(gradBr, distFromPixelToBr);
            float dotTl = Vector2.Dot(gradTl, distFromPixelToTl);
            float dotTr = Vector2.Dot(gradTr, distFromPixelToTr);

            gridUV = quintic(gridUV);

            float b = MathHelper.Lerp(dotBl, dotBr, gridUV.X);
            float t = MathHelper.Lerp(dotTl, dotTr, gridUV.X);
            float perlin = MathHelper.Lerp(b, t, gridUV.Y);

            perlin += 0.1f;

            return perlin;
        }

        Vector2 Frac(Vector2 p)
        {
            return p - new Vector2((float)Math.Floor(p.X), (float)Math.Floor(p.Y));
        }

        Vector2 result;
        Vector2 p1;
        Vector2 p2;
        Vector2 quintic(Vector2 p)
        {
            // return p * p * p * (10.0f + p * (-15.0f + p * 6.0f));
            p1 = new Vector2(-15 + p.X * 6, -15 + p.Y * 6);
            p2 = new Vector2(10 + p.X * p1.X, 10 + p.Y * p1.Y);
            result = p * p * p * p2;

            return result;
        }

        Vector2 randomGradient(Vector2 p)
        {
            p = p + offset;
            float x = Vector2.Dot(p, new Vector2(123.4f, 234.5f));
            float y = Vector2.Dot(p, new Vector2(234.5f, 335.6f));
            Vector2 gradient = new Vector2(x, y);
            gradient = new Vector2((float)Math.Sin(gradient.X),(float)Math.Sin(gradient.Y));
            gradient = gradient * 43758.5453f;
            gradient = new Vector2((float)Math.Sin(gradient.X + spin),(float)Math.Sin(gradient.Y + spin));
            return gradient;
        }
    }
}
