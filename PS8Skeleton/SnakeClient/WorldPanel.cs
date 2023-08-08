using IImage = Microsoft.Maui.Graphics.IImage;
#if MACCATALYST
using Microsoft.Maui.Graphics.Platform;
#else
using Microsoft.Maui.Graphics.Win2D;
#endif
using System.Reflection;
using Model;
using Windows.UI.Input.Inking;
using System.Diagnostics;
using Microsoft.Maui.Graphics.Text;
using Microsoft.Maui.Graphics;
using System.Linq;

namespace SnakeGame;
public class WorldPanel : ScrollView, IDrawable
{
    //Most of this came included with the skeleton.
    private IImage wallImage;
    private IImage background;

    //Array for storing each frame image of explosion animation
    private IImage[] explosionImage = new IImage[6];

    //Size of the entire world.
    private int viewSize;
    //Game controller from which this class takes the current world and player ID.
    private GameController gameController;

    private GraphicsView graphicsView = new();
    private delegate void ObjectDrawer(object o, ICanvas canvas);

    private bool initializedForDrawing = false;

    //List of colors used so that 8 snakes have unique files.
    private Color[] ColorWheel = { Colors.Red, Colors.Blue, Colors.Ivory, Colors.Orange, Colors.Yellow, Colors.Pink, Colors.Purple, Colors.Grey, Colors.Brown };

#if MACCATALYST
    private IImage loadImage(string name)
    {
        Assembly assembly = GetType().GetTypeInfo().Assembly;
        string path = "SnakeGame.Resources.Images";
        return PlatformImage.FromStream(assembly.GetManifestResourceStream($"{path}.{name}"));
    }
#else
    private IImage loadImage(string name)
    {
        Assembly assembly = GetType().GetTypeInfo().Assembly;
        string path = "SnakeGame.Resources.Images";
        var service = new W2DImageLoadingService();
        return service.FromStream(assembly.GetManifestResourceStream($"{path}.{name}"));
    }
#endif

    public WorldPanel()
    {
        BackgroundColor = Colors.Black;
        graphicsView.Drawable = this;
        graphicsView.HeightRequest = 900;
        graphicsView.WidthRequest = 900;
        graphicsView.BackgroundColor = Colors.Black;
        this.Content = graphicsView;
    }

    /// <summary>
    /// Creates a world panel that can access the information from the given GameController
    /// </summary>
    /// <param name="gc"></param>
    public void CreateFromWorld(GameController gc)
    {
        gameController = gc;
        viewSize = gc.GetWorld().Size;
        graphicsView.HeightRequest = viewSize;
        graphicsView.WidthRequest = viewSize;
    }

    private void InitializeDrawing()
    {
        wallImage = loadImage("WallSprite.png");
        background = loadImage("Background.png");

        for (int i = 0; i < explosionImage.Length; i++)
        {
            explosionImage[i] = loadImage("explosion" + (i + 1) + ".png");
        }
        initializedForDrawing = true;
    }

    public void Invalidate()
    {
        graphicsView.Invalidate();
    }

    /// <summary>
    /// Draws the items found in world.
    /// </summary>
    /// <param name="canvas"></param>
    /// <param name="dirtyRect"></param>
    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        if (!initializedForDrawing)
            InitializeDrawing();

        canvas.ResetState();

        //If there is no controller or world yet, there is nothing to draw.  Could draw a menu screen here with an else statement!
        if (gameController != null && gameController.HasWorld())
        {
            //These variables aren't needed, but make this much more readable.
            ClientWorld theWorld = gameController.GetWorld();
            int playerID = gameController.GetPlayerID();

            //Lock used to prevent modification of the world during drawing.
            lock (theWorld)
            {
                float playerX = 0;
                float playerY = 0;

                //Follow the player.
                if (theWorld.Snakes.ContainsKey(playerID))
                {
                    Snake playerSnake = theWorld.Snakes[playerID];
                    playerX = (float)playerSnake.body[playerSnake.body.Count - 1].X;
                    playerY = (float)playerSnake.body[playerSnake.body.Count - 1].Y;
                }
                canvas.Translate(-playerX + (900 / 2), -playerY + (900 / 2));

                if (!initializedForDrawing)
                    InitializeDrawing();

                //Draw the background, then everything else!
                canvas.DrawImage(background, -theWorld.Size / 2, -theWorld.Size / 2, theWorld.Size, theWorld.Size);

                foreach (Wall wall in theWorld.Walls.Values)
                {
                    DrawObjectWithTransform(canvas, wall, 0, 0, 0, WallDrawer);
                }

                foreach (Snake snake in theWorld.Snakes.Values)
                {
                    DrawObjectWithTransform(canvas, snake, 0, 0, 0, SnakeDrawer);
                }

                foreach (Powerup pwrup in theWorld.Powerups.Values)
                {
                    DrawObjectWithTransform(canvas, pwrup, pwrup.loc.X, pwrup.loc.Y, 0, PowerupDrawer);
                }

                foreach (Explosion explosion in theWorld.Explosions.Values)
                {
                    DrawObjectWithTransform(canvas, explosion, 0, 0, 0, ExplosionDrawer);
                }
            }
        }
    }

    /// <summary>
    /// Private helper used to draw a wall.
    /// </summary>
    /// <param name="o"></param>
    /// <param name="canvas"></param>
    private void WallDrawer(object o, ICanvas canvas)
    {
        Wall wall = o as Wall;
        float dX = (float)(wall.p1.X - wall.p2.X);
        float dY = (float)(wall.p1.Y - wall.p2.Y);

        if (dX == 0)
        {
            //Same X coordinates.  Vertically Alligned.
            for (float i = 0; i <= dY; i += 50)
            {
                canvas.DrawImage(wallImage, (float)wall.p1.X - 25, (float)wall.p1.Y - i - 25, 50, 50);
            }
            for (float i = 0; i >= dY; i -= 50)
            {
                canvas.DrawImage(wallImage, (float)wall.p1.X - 25, (float)wall.p1.Y - i - 25, 50, 50);
            }
        }
        else
        {
            //Same Y coordinates.  Horizontally Alligned.
            for (float i = 0; i <= dX; i += 50)
            {
                canvas.DrawImage(wallImage, (float)wall.p1.X - i - 25, (float)wall.p1.Y - 25, 50, 50);
            }
            for (float i = 0; i >= dX; i -= 50)
            {
                canvas.DrawImage(wallImage, (float)wall.p1.X - i - 25, (float)wall.p1.Y - 25, 50, 50);
            }
        }

    }

    /// <summary>
    /// Private helper used to draw a snake.  This also draws the snake's score above the snake.
    /// </summary>
    /// <param name="o"></param>
    /// <param name="canvas"></param>
    private void SnakeDrawer(object o, ICanvas canvas)
    {
        Snake s = o as Snake;
        if (!s.alive)
        {
            return;
        }

        canvas.StrokeSize = 10;
        canvas.StrokeColor = ColorWheel[s.snake % 8];

        for (int i = 0; i < s.body.Count - 1; i++)
        {
            Vector2D p1 = s.body[i];
            Vector2D p2 = s.body[i + 1];
            if ((int)(p2.X - p1.X) == 0 || (int)(p2.Y - p1.Y) == 0)
                canvas.DrawRoundedRectangle((float)p1.X, (float)p1.Y, (float)(p2.X - p1.X), (float)(p2.Y - p1.Y), 10);
        }

        canvas.DrawString(s.name + ": " + s.score, (float)s.body[s.body.Count - 1].X, (float)s.body[s.body.Count - 1].Y - 20, HorizontalAlignment.Center);
    }

    /// <summary>
    /// Private helper for drawing explosion animation upon snake's death
    /// </summary>
    /// <param name="o"></param>
    /// <param name="canvas"></param>
    private void ExplosionDrawer(object o, ICanvas canvas)
    {
        Explosion e = o as Explosion;

        int frame = e.NextFrame();
        if (frame <= 6)
        {
            //draw each frame of the explosion at the snake's head positon
            canvas.DrawImage(explosionImage[frame - 1], (float)e.pos.X - 16, (float)e.pos.Y - 16, 32, 32);
            frame++;
        }
        //Animation completed, remove from the world.
        else
            gameController.GetWorld().SetExplosion(e);

    }

    /// <summary>
    /// Private helper to draw the powerup.
    /// </summary>
    /// <param name="o"></param>
    /// <param name="canvas"></param>
    private void PowerupDrawer(object o, ICanvas canvas)
    {
        Powerup p = o as Powerup;
        
        int width = 16;
        canvas.FillColor = Colors.Yellow;
        canvas.FillEllipse(-(width / 2), -(width / 2), width, width);

        width = 10;
        canvas.FillColor = Colors.Green;

        canvas.FillEllipse(-(width / 2), -(width / 2), width, width);
    }

    /// <summary>
    /// Helper used to draw an object at a given position and angle using the object's draw method.
    /// </summary>
    /// <param name="canvas"></param>
    /// <param name="o"></param>
    /// <param name="worldX"></param>
    /// <param name="worldY"></param>
    /// <param name="angle"></param>
    /// <param name="drawer"></param>
    private void DrawObjectWithTransform(ICanvas canvas, object o, double worldX, double worldY, double angle, ObjectDrawer drawer)
    {
        canvas.SaveState();

        canvas.Translate((float)worldX, (float)worldY);
        canvas.Rotate((float)angle);
        drawer(o, canvas);

        canvas.RestoreState();
    }

}
