using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

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

                _effect.Parameters["spin"].SetValue(spin);
            _effect.Parameters["scale"].SetValue(scale);

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

            _spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
