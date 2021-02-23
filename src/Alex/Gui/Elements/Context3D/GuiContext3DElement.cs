﻿using Alex.Api;
using Alex.API.Gui.Elements;
using Alex.API.Gui.Graphics;
using Alex.API.Utils;
using Alex.Gamestates;
using Alex.Graphics.Camera;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Gui.Elements.Context3D
{
    public class GuiContext3DElement : GuiElement
    {
        public GuiContext3DCamera Camera { get; protected set; }

        public virtual PlayerLocation TargetPosition { get; set; } = new PlayerLocation(Vector3.Zero);

        private Rectangle _previousBounds;

        public IGuiContext3DDrawable Drawable
        {
            get => _drawable;
            set
            {
                _drawable = value;
                _canRender = _drawable != null;
            }
        }

        private bool _canRender;
        private IGuiContext3DDrawable _drawable;

        public GuiContext3DElement(IGuiContext3DDrawable drawable) : this()
        {
            Drawable = drawable;
        }

        public GuiContext3DElement()
        {
           // Background = Color.Transparent;
            TargetPosition = new PlayerLocation(Vector3.Zero);
            Camera = new GuiContext3DCamera(TargetPosition);
            ClipToBounds = false;
            Background = GuiTextures.PanelGeneric;
        }


        protected sealed override void OnDraw(GuiSpriteBatch graphics, GameTime gameTime)
        {
            base.OnDraw(graphics, gameTime);

            if (_canRender)
            {
                var renderArgs = new RenderArgs()
                {
                    GraphicsDevice = graphics.SpriteBatch.GraphicsDevice,
                    SpriteBatch = graphics.SpriteBatch,
                    GameTime = gameTime,
                    Camera = Camera,
                };

                //var viewport = args.Graphics.Viewport;
                //args.Graphics.Viewport = new Viewport(RenderBounds);
                //graphics.End();

                var rast = new RasterizerState()
                {
                    CullMode = CullMode.CullClockwiseFace,
                    FillMode = FillMode.Solid,
                    DepthClipEnable = false,
                    ScissorTestEnable = false
                };
                
                using (var context = graphics.BranchContext(BlendState.AlphaBlend, DepthStencilState.Default,
                    rast, SamplerState.PointWrap))
                {
                    var bounds = RenderBounds;

                    bounds.Inflate(-3, -3);

                    var p  = graphics.Project(bounds.Location.ToVector2());
                    var p2 = graphics.Project(bounds.Location.ToVector2() + bounds.Size.ToVector2());

                    var newViewport = Camera.Viewport;
                    newViewport.X      = (int) p.X;
                    newViewport.Y      = (int) p.Y;
                    newViewport.Width  = (int) (p2.X - p.X);
                    newViewport.Height = (int) (p2.Y - p.Y);
                    //newViewport.MaxDepth = 128f;
                   // newViewport.MinDepth = 0.01f;
                    //newViewport.

                    Camera.Viewport = newViewport;

                    context.Viewport = Camera.Viewport;

                    graphics.Begin();

                    Drawable?.DrawContext3D(renderArgs, GuiRenderer);
                    // Entity.Render(renderArgs);
                    //  EntityModelRenderer?.Render(renderArgs, EntityPosition);

                    graphics.End();
                }
            }
        }

        protected sealed override void OnUpdate(GameTime gameTime)
        {
            base.OnUpdate(gameTime);

            if (_canRender)
            {
                Camera.LookAt = TargetPosition;
                var bounds = Screen.RenderBounds;

                if (bounds != _previousBounds)
                {
                    //var c = bounds.Center;
                    //Camera.RenderPosition = new Vector3(c.X, c.Y, 0.0f);
                    //Camera.UpdateProjectionMatrix();
                    //Camera.UpdateAspectRatio((float) bounds.Width / (float) bounds.Height);
                    //Camera.UpdateAspectRatio(bounds.Width / (float) bounds.Height);
                    _previousBounds = bounds;
                }

                var updateArgs = new UpdateArgs()
                {
                    GraphicsDevice = Alex.Instance.GraphicsDevice,
                    GameTime = gameTime,
                    Camera = Camera
                };

                //Camera.MoveTo(TargetPosition, Vector3.Zero);

                Drawable.UpdateContext3D(updateArgs, GuiRenderer);
                //EntityModelRenderer?.Update(updateArgs, EntityPosition);
            }
        }

        public class GuiContext3DCamera : Camera
        {
            public Viewport Viewport
            {
                get => _viewport;
                set
                {
                    _viewport = value;
                    
                    UpdateAspectRatio(_viewport.AspectRatio);
                }
            }

           // private Vector3 _targetPositionOffset = new Vector3(0f, 0f, 0f);
            //private Vector3 _cameraPositionOffset = new Vector3(0f, 0f, 1f);
            private Viewport _viewport;

            public GuiContext3DCamera(Vector3 basePosition) : base()
            {
                Viewport = new Viewport(256, 128, 128, 256, 0.01f, 128f);
                Position = basePosition;
                Rotation = Vector3.Zero;
                FOV = 75.0f;
                
                SetRenderDistance(128);
            }

            public Vector3 LookAt
            {
                get => Target;
                set => Target = value;
            }
            
            protected override void UpdateViewMatrix()
            {
                MCMatrix rotationMatrix = (MCMatrix.CreateRotationX(Rotation.X) *
                                           MCMatrix.CreateRotationY(Rotation.Y));
                
               // Target = Position;

                Direction = Vector3.Transform(Vector3.Backward, rotationMatrix);

                ViewMatrix = MCMatrix.CreateLookAt(Position, Vector3.Zero, Vector3.Up);
                Frustum = new BoundingFrustum(ViewMatrix * ProjectionMatrix);
            }

            public override void UpdateProjectionMatrix()
            {
                //ProjectionMatrix = Matrix.CreatePerspectiveOffCenter(Viewport.RenderBounds, NearDistance, FarDistance);
                ProjectionMatrix =
                    MCMatrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(FOV), Viewport.AspectRatio,
                        NearDistance, FarDistance);
            }

            //public override void UpdateProjectionMatrix()
            //{
            //    var bounds = _modelView.RenderBounds;
            //    ProjectionMatrix =
            //        Matrix.CreatePerspective(Viewport.Width, Viewport.Height, Viewport.MinDepth, Viewport.MaxDepth);
            //    //ProjectionMatrix = Matrix.CreateOrthographicOffCenter(Viewport.RenderBounds, NearDistance, FarDistance);// * Matrix.CreateTranslation(new Vector3(bounds.Location.ToVector2(), -1f));
            //    //ProjectionMatrix = Matrix.CreatePerspectiveOffCenter(_modelView.RenderBounds, NearDistance, FarDistance);
            //}
        }
    }
}