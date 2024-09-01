//Javelin Studios
//June 18, 2024
//Player 223 is a 2D platformer where the player is able to build their own map in "creative" mode, or play the premade parkour map.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.WebRequestMethods;

namespace PLAYER_223
{
    public partial class frmGame : Form
    {
        public frmGame()
        {
            InitializeComponent();
        }
        //Bitmaps for all tiles. 
        Bitmap backbuffer;
        Bitmap environment;
        Bitmap bmpSnow;
        Bitmap bmpBare;
        Bitmap bmpStone;
        Bitmap bmpMud;
        Bitmap bmpGravel;
        Bitmap bmpice;
        Bitmap bmpAir;
        Bitmap bmpChar;
        Bitmap bmpsand;
        Bitmap bmpGrass;
        Bitmap bmpBrick;
        Bitmap bmpWood;
        Bitmap bmpSandStone;
        Bitmap bmpTrophy;
        Bitmap bmpLava;
        frmGraphics frmG = new frmGraphics();
        Rectangle rectFrame;  //Used to get specific movement frames from the player's bitmap. 
        int tileSize = 64; //tilesize
        int cornerX, cornerY; //Coordinates of map associated with the top left corner of the screen.
        Rectangle rectPlayerPos;      //rectangle for player position starting at top left corner.
        int[,] map = new int[135, 52]; //Array to hold all tiles of map (not including backdrop).
        int direction;
        int movementFrame = 5;              //Player animation frame starts at 5.
        int speed;               
        int mouseX, mouseY;
        bool forwardAnimation = true;       //Used to cycle the animation frames back and forth. 
        bool moving = false;
        bool buildMode = false; //Used to allow us to edit the map whenever we want. Starts false.
        //Variables used to measure player's position on the map and control collisions.
        int curTileX, curTileY;           
        int otherTileX; //Used when the player is standing in between two tiles.
        int groundTileY; 
        int roofTileY;
        //Variables Used for gravity.
        int vertVel;
        bool ableToJump = true;
        bool jumped;
        bool spaceBar;
        bool pressU = true;//used to allow u to not be pressed midgame
        int block = 0;
        //timer things
        int seconds = 0;
        int minutes = 0;
        //Loading Screen things
        bool loading = false; //used to allow loading screen to come up before the map
        bool gen = false; //used to determine gender
        int screen = 1;
        bool control = true; // control panel tings
        bool bothclicked = false;//used to determine if the the loading screen character and mode are selected
        bool creative; //used to determine if creative mode is on
        int appearance = 0;//used to determine which look the person selected
        int backgselect = 0;//used to determine which background was selected
        bool torf = true;
        //Assigns left and right a number so that it is easier to remember. 
        enum dir
        {
            left,
            right,
        }
        //Reloads map and player according to new values. 
        private void getMap()
        {
            if (creative)
            {

                Graphics gEnvironment = Graphics.FromImage(environment);
                Graphics g = this.CreateGraphics();
                Graphics gback = Graphics.FromImage(backbuffer);
                //Sets the background image to the image selected by the player.
                if (loading == true && backgselect == 0)
                {
                    gback.DrawImage(frmG.picBack.Image, 0, 0, this.Width, this.Height);
                }
                else if (loading == true && backgselect == 1)
                {
                    gback.DrawImage(frmG.picTree.Image, 0, 0, this.Width, this.Height);
                }
                else if (loading == true && backgselect == 2)
                {
                    gback.DrawImage(frmG.picDesertBack.Image, 0, 0, this.Width, this.Height);
                }
                else if (loading == true && backgselect == 3)
                {
                    gback.DrawImage(frmG.picNighttime.Image, 0, 0, this.Width, this.Height);
                }
                gback.DrawImage(environment, cornerX, cornerY, environment.Width, environment.Height);                                                        //Draws the map starting at the top left corner coordinates.
                rectFrame = new Rectangle(rectPlayerPos.Width * movementFrame, rectPlayerPos.Height * direction, rectPlayerPos.Width, rectPlayerPos.Height);  //Determines which frame of the player to draw.
                gback.DrawImage(bmpChar, rectPlayerPos, rectFrame, GraphicsUnit.Pixel);                                                                        //Draws player frame at the player position.
                gback.Dispose();
                g.DrawImage(backbuffer, 0, 0);                                                                                                                //Draws backbuffer onto form.
                g.Dispose();
            }
            else
            {
                Graphics g = this.CreateGraphics();
                Graphics gback = Graphics.FromImage(backbuffer);
                gback.DrawImage(frmG.picHell.Image, 0, 0, this.Width, this.Height);
                gback.DrawImage(environment, cornerX, cornerY, environment.Width, environment.Height);                                                        //Draws the map starting at the top left corner coordinates.
                rectFrame = new Rectangle(rectPlayerPos.Width * movementFrame, rectPlayerPos.Height * direction, rectPlayerPos.Width, rectPlayerPos.Height);  //Determines which frame of the player to draw.
                gback.DrawImage(bmpChar, rectPlayerPos, rectFrame, GraphicsUnit.Pixel);                                                                        //Draws player frame at the player position.
                gback.Dispose();
                g.DrawImage(backbuffer, 0, 0);                                                                                                                //Draws backbuffer onto form.
                g.Dispose();
            }
        }
        //Gets current coordinates of player on map. 
        private void getPos()
        {
            //Coordinates of curTile is the coordinates of the players feet.
            int posX = rectPlayerPos.X - cornerX;
            int posY = rectPlayerPos.Y - cornerY;
            curTileX = (posX - posX % tileSize) / tileSize;
            otherTileX = curTileX;
            //If the player is between two tiles, set otherTileX to one tile to the right of curTileX.
            if (posX % tileSize > 0)
            {
                otherTileX++;
            }
            curTileY = (posY - posY % tileSize) / tileSize + 1;
        }
        private void frmGame_Paint(object sender, PaintEventArgs e) //So that the backbuffer is redrawn whenever the form size is changed.
        {
            Graphics g = e.Graphics;
            g.DrawImage(backbuffer, 0, 0);
        }

        private void frmGame_KeyUp(object sender, KeyEventArgs e)
        {
            //Knows when the spacebar is released.
            if (e.KeyCode == Keys.Space)
            {
                spaceBar = false;
            }
            //Stops walking when the key is released.
            else if ((e.KeyCode == Keys.D && direction == (int)dir.right) || (e.KeyCode == Keys.A && direction == (int)dir.left))
            {
                moving = false;
                speed = 0; //Resets speed.
                forwardAnimation = true;
            }
        }

        private void moveTimer_Tick(object sender, EventArgs e) //Handles movement and walking animations.
        {
            //restarts the walking animation loop, to loop through the frames starting from the opposite end. 
            if (movementFrame >= 4 || movementFrame <= 0)
            {
                //If it was forward, start from frame 4.
                if (forwardAnimation)
                {
                    forwardAnimation = false;
                    movementFrame = 4;
                }
                //Otherwise, start from frame 0.
                else
                {
                    forwardAnimation = true;
                    movementFrame = 0;
                }
            }
            getPos();
            //Moves the map in the opposite direction of the player to move the player through the environment.
            if (moving)
            {
                //Right vs left movement.
                if (direction == (int)dir.right && map[curTileX + 1, curTileY] == 0 && map[curTileX + 1, curTileY - 1] == 0 && curTileX + 1 != 0 && curTileX + 1 != map.GetLength(0) - 1)
                {
                    cornerX -= speed;
                    getPos();
                    //If the player hits a wall, they will likely be in the wall to some degree. This code pulls them back out.
                    if (map[curTileX + 1, curTileY] != 0 || map[curTileX + 1, curTileY - 1] != 0)
                    {
                        cornerX = (curTileX - 11) * tileSize * -1;
                    }
                }
                else if (direction == (int)dir.left && map[otherTileX - 1, curTileY] == 0 && map[otherTileX - 1, curTileY - 1] == 0 && otherTileX - 1 != 0 && otherTileX - 1 != map.GetLength(0) - 1)
                {
                    cornerX += speed;
                    getPos();
                    //If the player hits a wall, they will likely be in the wall to some degree. This code pulls them back out.
                    if (map[otherTileX - 1, curTileY] != 0 || map[otherTileX - 1, curTileY - 1] != 0)
                    {
                        cornerX = (otherTileX - 11) * tileSize * -1;
                    }
                }
            }
            //Finds Y coordinate of the first solid tile underneath the player.
            groundTileY = curTileY;
            do
            {
                groundTileY++;
            } while (groundTileY < map.GetLength(1) - 1 && map[curTileX, groundTileY] == 0 && map[otherTileX, groundTileY] == 0);
            //Gravity code:
            //If the tile underneath the player is higher than the ground tile, or if the player jumped.
            if (curTileY + 1 < groundTileY || jumped == true)
            {
                //If they are traveling upward, find the first solid tile above their head.
                if (vertVel > 0)
                {
                    roofTileY = curTileY;
                    do
                    {
                        roofTileY--;
                    } while (map[curTileX, roofTileY] == 0 && map[otherTileX, roofTileY] == 0 && roofTileY > 0);
                }
                //Moves the player according to vertical velocity.
                ableToJump = false;
                movementFrame = 6;
                cornerY += vertVel;
                getPos();
                //If the player moved up and moving the player resulted in hitting or passing the roofTile,
                //set the player's position to one tile below the roof tile.
                if (vertVel > 0 && curTileY - 1 <= roofTileY)
                {
                    cornerY = (roofTileY - 7) * tileSize * -1 + 1; //Adds 4 to cornerY just to add an effect where it still looks like the player tried to jump even when there's no room.
                    vertVel = 0;
                }
                vertVel -= tileSize / 2;
                //If moving the player down resulted in bringing them to the ground OR lower,
                //set their position to one tile above the ground tile. 
                if (curTileY + 1 >= groundTileY)
                {
                    //At this point they are on the ground.
                    cornerY = (groundTileY - 10) * tileSize * -1;
                    movementFrame = 5;
                    //If they haven't let go of spacebar.
                    if (spaceBar)
                    {
                        ableToJump = false;
                        vertVel = 96;
                    }
                    else
                    {
                        jumped = false;
                        vertVel = 0;
                        ableToJump = true;
                    }
                }
            }
            //If it gets to this code, it means the player is on the ground.
            else
            {
                if (moving == false)
                {
                    moveTimer.Enabled = false;
                    movementFrame = 5;
                }
                //Changes the movement frame according to the direction of the animation cycle.
                else if (forwardAnimation)
                {
                    vertVel = 0;
                    movementFrame++;
                }
                else
                {
                    vertVel = 0;
                    movementFrame--;
                }
                ableToJump = true;
            }
            //Keeps increasing speed until it reaches the max speed of 30.
            if (speed < 30)
            {
                speed += 5;
            }
            getMap();
            getPos();
            //Checks if the player is now touching the trophy. 
            if (map[curTileX + 1, curTileY] == 12 || map[curTileX + 1, curTileY - 1] == 12 || map[otherTileX - 1, curTileY] == 12 || map[otherTileX - 1, curTileY - 1] == 12)
            {
                newGame();
                MessageBox.Show("YOU WIN!");
            }
            //Checks if the player is now touching a lava tile. 
            if (map[curTileX + 1, curTileY] == 13 || map[curTileX + 1, curTileY - 1] == 13 || map[otherTileX - 1, curTileY] == 13 || map[otherTileX - 1, curTileY - 1] == 13 || map[curTileX, curTileY + 1] == 13 || map[otherTileX, curTileY + 1] == 13)
            {
                newGame();
                MessageBox.Show("YOU DIED.");
            }
        }
        private void timer1_Tick(object sender, EventArgs e)//timer
        {
            seconds++;
            if (seconds == 60)// allows minutes to happen
            {
                seconds = 0;
                minutes++;
            }
            if (seconds <= 9)//if seconds = 9 or below has 0 else just displays seconds
            {
                lblTime.Text = $"{minutes}:0{seconds}";
            }
            else
            {
                lblTime.Text = $"{minutes}:{seconds}";
            }
            if (seconds == 10)//hide guide picture box at 10
            {
                picPressBut.Hide();
            }
        }
        private void picMale_click(object sender, EventArgs e)//male character select
        {
            bothclicked = true;
            gen = true;
            picMale.BackColor = Color.DarkGray;
            picFemale.BackColor = Color.Gray;
            screen = 2;
            bmpChar = new Bitmap(frmG.picGuyRags.Image);
            bmpChar.MakeTransparent(Color.Fuchsia);
        }
        private void picNew_Click(object sender, EventArgs e)//new button
        {
            if (screen == 2 && creative)//hides loading screen and shows all the icons to select a map 
            {
                picMale.Hide();
                picFemale.Hide();
                picNew.Hide();
                picLogo.Hide();
                picSelect.Hide();
                picX.Hide();
                picSave.Hide();
                picCreative.Hide();
                picParkour.Hide();
                picControls.Hide();
                picSelectGame.Hide();
                picNightimeText.Show();
                picNighttimePick.Show();
                picSnowPick.Show();
                picDesertPick.Show();
                picTreePick.Show();
                picTreeText.Show();
                picDesertText.Show();
                picSnowTExt.Show();
                picMale.BackColor = Color.Gray;
                picFemale.BackColor = Color.Gray;
                picCreative.BackColor = Color.Gray;
                picParkour.BackColor = Color.Gray;
                picBackgroundSelect.Show();
                torf = false;
                loading = false;
                buildMode = false;
                lblTime.Text = $"{minutes}:0{seconds}";
                screen = 0;
                block = 0;
                appearance = 0;
                picSnowSelect.BackColor = Color.Black;
                picWoodSelect.BackColor = Color.Black;
                picSandSelect.BackColor = Color.Black;
                picGravelSelect.BackColor = Color.Black;
                picGrassSelect.BackColor = Color.Black;
                picSandStoneSelect.BackColor = Color.Black;
                picIceSelect.BackColor = Color.Black;
                picStoneSelect.BackColor = Color.Black;
                picBrickSelect.BackColor = Color.Black;
                picDelete.BackColor = Color.Black;
                picMudSelect.BackColor = Color.Black;
            }
            else if (screen == 2 && creative == false)//hides loading screen and lets you enter the game by pressing u
            {
                picSelectGame.Hide();
                picMale.Hide();
                picFemale.Hide();
                picNew.Hide();
                picLogo.Hide();
                picSelect.Hide();
                picX.Hide();
                picControls.Hide();
                picSave.Hide();
                picCreative.Hide();
                picMale.BackColor = Color.Gray;
                picFemale.BackColor = Color.Gray;
                picCreative.BackColor = Color.Gray;
                picParkour.BackColor = Color.Gray;
                picParkour.Hide();
                buildMode = false;
                loading = true;
                torf = false;
                screen = 0;
                pressU = true;
                picStart.Show();
            }
        }
        private void picFemale_Click(object sender, EventArgs e)//female char select
        {
            bothclicked = true;
            gen = false;
            screen = 2;
            picFemale.BackColor = Color.DarkGray;
            picMale.BackColor = Color.Gray;
            bmpChar = new Bitmap(frmG.picGirlRags.Image);
            bmpChar.MakeTransparent(Color.Fuchsia);
        }
        private void picX_Click(object sender, EventArgs e)//exit button
        {
            Application.Exit();
        }

        private void picSave_Click(object sender, EventArgs e)//save map
        {
            if (creative)
            {
                try
                {
                    //Prepares file to be overwritten.
                    string filePath = Application.StartupPath + "\\creative.txt";
                    FileStream fs = new FileStream(filePath, FileMode.Create);
                    StreamWriter myFile = new StreamWriter(fs);
                    //Writes the value of each array element into the file.
                    for (int y = 0; y < map.GetLength(1); y++)
                    {
                        for (int x = 0; x < map.GetLength(0); x++) //Loops through elements row by row.
                        {
                            myFile.WriteLine(map[x, y]);
                        }
                    }
                    myFile.Close();
                    MessageBox.Show("MAP SAVED.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                MessageBox.Show("You need to be in creative mode to save the map.");
            }
        }
        private void hideMapPicks()//hides all the map selection screen picboxes
        {
            picTreePick.Hide();
            picDesertPick.Hide();
            picSnowPick.Hide();
            picTreeText.Hide();
            picDesertText.Hide();
            picNightimeText.Hide();
            picNighttimePick.Hide();
            picSnowTExt.Hide();
            picBackgroundSelect.Hide();
            loading = true;
            torf = false;
            screen = 0;
            picStart.Show();
            pressU = true;
        }

        private void picTreePick_Click(object sender, EventArgs e)//if you select tree hide all icons and start game
        {
            hideMapPicks();
            backgselect = 1;
        }

        private void picSnowPick_Click(object sender, EventArgs e)//if you select snow hide all icons and start game
        {
            hideMapPicks();
            backgselect = 0;
        }
        private void picDesertPick_Click(object sender, EventArgs e)//if you select desert hide all icons and start game
        {
            hideMapPicks();
            backgselect = 2;
        }
        private void picCreative_Click(object sender, EventArgs e)
        {
            if (bothclicked)//makes it so you have to select a char before selecting gamemode
            {
                creative = true;
                picCreative.BackColor = Color.DarkGray;
                picParkour.BackColor = Color.Gray;
            }
        }

        private void picParkour_Click(object sender, EventArgs e)
        {
            if (bothclicked)//makes it so you have to select a char before selecting gamemode
            {
                creative = false;
                picCreative.BackColor = Color.Gray;
                picParkour.BackColor = Color.DarkGray;
            }
        }

        private void picControls_Click(object sender, EventArgs e)
        {
            if (control)//allows the controls to come up without hindering anything else
            {
                picMale.Hide();
                picFemale.Hide();
                picNew.Hide();
                picLogo.Hide();
                picSelect.Hide();
                picX.Hide();
                picCreative.Hide();
                picParkour.Hide();
                picSave.Hide();
                picSelectGame.Hide();
                picControlsPanel.Show();
                control = false;
            }
            else if (control == false)
            {
                picMale.Show();
                picFemale.Show();
                picNew.Show();
                picLogo.Show();
                picSelect.Show();
                picX.Show();
                picControls.Show();
                picCreative.Show();
                picParkour.Show();
                picSave.Show();
                picSelectGame.Show();
                picControlsPanel.Hide();
                control = true;
            }
        }

        private void picNighttimePick_Click(object sender, EventArgs e)//pick nighttime
        {
            hideMapPicks();
            backgselect = 3;
        }

        private void frmGame_MouseClick(object sender, MouseEventArgs e)//allows you to click to select a block
        {
            Graphics gEnvironment = Graphics.FromImage(environment);
            if (buildMode == true && block > 0 && creative)
            {
                mouseX = (e.X - cornerX - (e.X - cornerX) % tileSize);
                mouseY = (e.Y - cornerY - (e.Y - cornerY) % tileSize);
                if (e.Button == MouseButtons.Left && mouseY / tileSize < map.GetLength(1) && mouseX / tileSize < map.GetLength(0))
                {
                    if (block == 1)
                    {
                        gEnvironment.DrawImage(bmpSnow, mouseX, mouseY, tileSize, tileSize);
                        map[mouseX / tileSize, mouseY / tileSize] = 1;
                    }
                    else if (block == 2)
                    {
                        gEnvironment.DrawImage(bmpMud, mouseX, mouseY, tileSize, tileSize);
                        map[mouseX / tileSize, mouseY / tileSize] = 2;
                    }
                    else if (block == 3)
                    {
                        gEnvironment.DrawImage(bmpStone, mouseX, mouseY, tileSize, tileSize);
                        map[mouseX / tileSize, mouseY / tileSize] = 3;
                    }
                    else if (block == 4)
                    {
                        gEnvironment.DrawImage(bmpSandStone, mouseX, mouseY, tileSize, tileSize);
                        map[mouseX / tileSize, mouseY / tileSize] = 4;
                    }
                    else if (block == 5)
                    {
                        gEnvironment.DrawImage(bmpsand, mouseX, mouseY, tileSize, tileSize);
                        map[mouseX / tileSize, mouseY / tileSize] = 5;
                    }
                    else if (block == 6)
                    {
                        gEnvironment.DrawImage(bmpGrass, mouseX, mouseY, tileSize, tileSize);
                        map[mouseX / tileSize, mouseY / tileSize] = 6;
                    }
                    else if (block == 7)
                    {
                        gEnvironment.DrawImage(bmpice, mouseX, mouseY, tileSize, tileSize);
                        map[mouseX / tileSize, mouseY / tileSize] = 7;
                    }
                    else if (block == 8)
                    {
                        gEnvironment.DrawImage(bmpGravel, mouseX, mouseY, tileSize, tileSize);
                        map[mouseX / tileSize, mouseY / tileSize] = 8;
                    }
                    else if (block == 9)
                    {
                        gEnvironment.DrawImage(bmpWood, mouseX, mouseY, tileSize, tileSize);
                        map[mouseX / tileSize, mouseY / tileSize] = 9;
                    }
                    else if (block == 10)
                    {
                        gEnvironment.DrawImage(bmpBrick, mouseX, mouseY, tileSize, tileSize);
                        map[mouseX / tileSize, mouseY / tileSize] = 10;
                    }
                    else if (block == 11)
                    {
                        gEnvironment.DrawImage(bmpAir, mouseX, mouseY, tileSize, tileSize);
                        environment.MakeTransparent(Color.Fuchsia);
                        map[mouseX / tileSize, mouseY / tileSize] = 0;
                    }
                    else if (block == 12)
                    {
                        gEnvironment.DrawImage(bmpTrophy, mouseX, mouseY, tileSize, tileSize);
                        map[mouseX / tileSize, mouseY / tileSize] = 12;
                    }
                    else if (block == 13)
                    {
                        gEnvironment.DrawImage(bmpLava, mouseX, mouseY, tileSize, tileSize);
                        map[mouseX / tileSize, mouseY / tileSize] = 13;
                    }
                    gEnvironment.Dispose();
                        getMap();
                }
            }
        }
   

        private void Form1_Load(object sender, EventArgs e) //ON FORM LOAD.
        {            //fullscreen
            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;
            //loading screen picture boxe stuff         
            picTime.Hide();
            lblTime.Hide();
            picStart.Hide();
            picControlsPanel.Hide();
            picPressBut.Hide();
            picNightimeText.Hide();
            picNighttimePick.Hide();
            picTreePick.Hide();
            picSnowPick.Hide();
            picDesertPick.Hide();
            picTreeText.Hide();
            picDesertText.Hide();
            picSnowTExt.Hide();
            picBackgroundSelect.Hide();
            picSnowSelect.Hide();
            picWoodSelect.Hide();
            picSandSelect.Hide();
            picGravelSelect.Hide();
            picGrassSelect.Hide();
            picSandStoneSelect.Hide();
            picIceSelect.Hide();
            picStoneSelect.Hide();
            picBrickSelect.Hide();
            picDelete.Hide();
            picMudSelect.Hide();
            pic0.Hide();
            pic1.Hide();
            pic2.Hide();
            pic3.Hide();
            pic4.Hide();
            pic5.Hide();
            pic6.Hide();
            pic7.Hide();
            pic8.Hide();
            pic9.Hide();
            pic0.Hide();
            picMinus.Hide();
            //music
            SoundPlayer player = new SoundPlayer();
            player.SoundLocation = AppDomain.CurrentDomain.BaseDirectory + "\\P223MainTheme.wav";
            player.Play();
            //transparent
            picTime.BackColor = Color.Transparent;
            picParkour.BackColor = Color.DarkGray;
            backbuffer = new Bitmap(this.Width, this.Height * tileSize);
            //Creates bitmaps for tiles and character.
            bmpSnow = new Bitmap(frmG.picSnow.Image, tileSize, tileSize);
            bmpBare = new Bitmap(frmG.picBare.Image, tileSize, tileSize);
            bmpMud = new Bitmap(frmG.picMud.Image, tileSize, tileSize);
            bmpGravel = new Bitmap(frmG.picGravel.Image, tileSize, tileSize);
            bmpice = new Bitmap(frmG.picICe.Image, tileSize, tileSize);
            bmpStone = new Bitmap(frmG.picStone.Image, tileSize, tileSize);
            bmpAir = new Bitmap(frmG.picAir.Image, tileSize, tileSize);
            bmpsand = new Bitmap(frmG.picSand.Image, tileSize, tileSize);
            bmpGrass = new Bitmap(frmG.picGRASS.Image, tileSize, tileSize);
            bmpSandStone = new Bitmap(frmG.picSandStone.Image, tileSize, tileSize);
            bmpBrick = new Bitmap(frmG.picBrick.Image, tileSize, tileSize);
            bmpWood = new Bitmap(frmG.picWood.Image, tileSize, tileSize);
            bmpTrophy = new Bitmap(frmG.picTrophy.Image, tileSize, tileSize);
            bmpLava = new Bitmap(frmG.picLAVA.Image, tileSize, tileSize);
            moveTimer.Enabled = false;
            //Top left corner of Player is always at a position of 11 tiles right and 8 tiles down with a size of 1 x 2 tiles.
            rectPlayerPos = new Rectangle(11 * tileSize, 8 * tileSize, tileSize, 2 * tileSize);
            //Bottom left corner is 11 tiles right and 9 tiles down.
            curTileX = 11;
            curTileY = 9;
        }
        //When a key is pressed down.
        private void frmGame_KeyDown(object sender, KeyEventArgs e)
        {
            //Toggle build mode mode.
            if (screen == 0 && torf)
            {
                if (e.KeyCode == Keys.B)//turns you into a builder and lets you start building
                {
                    if (buildMode == false && creative)
                    {
                        buildMode = true;
                        picPressBut.Hide();
                        picSnowSelect.Show();
                        picWoodSelect.Show();
                        picSandSelect.Show();
                        picGravelSelect.Show();
                        picGrassSelect.Show();
                        picSandStoneSelect.Show();
                        picIceSelect.Show();
                        picStoneSelect.Show();
                        picBrickSelect.Show();
                        picDelete.Show();
                        picMudSelect.Show();
                        pic0.Show();
                        pic1.Show();
                        pic2.Show();
                        pic3.Show();
                        pic4.Show();
                        pic5.Show();
                        pic6.Show();
                        pic7.Show();
                        pic8.Show();
                        pic9.Show();
                        pic0.Show();
                        picMinus.Show();
                        bmpChar = new Bitmap(frmG.picBuildMode.Image);
                        bmpChar.MakeTransparent(Color.Fuchsia);
                    }
                    else if (buildMode && creative)
                    {
                        buildMode = false;
                        picSnowSelect.Hide();
                        picWoodSelect.Hide();
                        picSandSelect.Hide();
                        picGravelSelect.Hide();
                        picGrassSelect.Hide();
                        picSandStoneSelect.Hide();
                        picIceSelect.Hide();
                        picStoneSelect.Hide();
                        picBrickSelect.Hide();
                        picDelete.Hide();
                        picMudSelect.Hide();
                        pic0.Hide();
                        pic1.Hide();
                        pic2.Hide();
                        pic3.Hide();
                        pic4.Hide();
                        pic5.Hide();
                        pic6.Hide();
                        pic7.Hide();
                        pic8.Hide();
                        pic9.Hide();
                        pic0.Hide();
                        picMinus.Hide();
                        picSnowSelect.BackColor = Color.Black;
                        picWoodSelect.BackColor = Color.Black;
                        picSandSelect.BackColor = Color.Black;
                        picGravelSelect.BackColor = Color.Black;
                        picGrassSelect.BackColor = Color.Black;
                        picSandStoneSelect.BackColor = Color.Black;
                        picIceSelect.BackColor = Color.Black;
                        picStoneSelect.BackColor = Color.Black;
                        picBrickSelect.BackColor = Color.Black;
                        picDelete.BackColor = Color.Black;
                        picMudSelect.BackColor = Color.Black;
                        block = 0;
                        if (appearance == 0 && gen == true)
                        {
                            bmpChar = new Bitmap(frmG.picGuyRuby.Image);
                            bmpChar.MakeTransparent(Color.Fuchsia);
                        }
                        else if (appearance == 1 && gen == true)
                        {
                            bmpChar = new Bitmap(frmG.picGuyRags.Image);
                            bmpChar.MakeTransparent(Color.Fuchsia);
                        }
                        else if (appearance == 2 && gen == true)
                        {
                            bmpChar = new Bitmap(frmG.picGuyWood.Image);
                            bmpChar.MakeTransparent(Color.Fuchsia);
                        }
                        else if (appearance == 3 && gen == true)
                        {
                            bmpChar = new Bitmap(frmG.picGuyRuby.Image);
                            bmpChar.MakeTransparent(Color.Fuchsia);
                        }
                        else if (appearance == 0 && gen == false)
                        {
                            bmpChar = new Bitmap(frmG.picGirlRuby.Image);
                            bmpChar.MakeTransparent(Color.Fuchsia);
                        }
                        else if (appearance == 1 && gen == false)
                        {
                            bmpChar = new Bitmap(frmG.picGirlRags.Image);
                            bmpChar.MakeTransparent(Color.Fuchsia);
                        }
                        else if (appearance == 2 && gen == false)
                        {
                            bmpChar = new Bitmap(frmG.picGirlWood.Image);
                            bmpChar.MakeTransparent(Color.Fuchsia);
                        }
                        else if (appearance == 3 && gen == false)
                        {
                            bmpChar = new Bitmap(frmG.picGirlIron.Image);
                            bmpChar.MakeTransparent(Color.Fuchsia);
                        }
                    }
                    getMap();
                }
                else
                {
                    //Moving left and right.
                    if (e.KeyCode == Keys.D)
                    {
                        direction = (int)dir.right;
                        moving = true;
                    }
                    else if (e.KeyCode == Keys.A)
                    {
                        direction = (int)dir.left;
                        moving = true;
                    }
                    //Jumping. 
                    else if (e.KeyCode == Keys.Space && ableToJump)
                    {
                        getPos();
                        spaceBar = true;
                        jumped = true;
                        ableToJump = false;
                        vertVel = 96;
                    }
                    moveTimer.Enabled = true;
                }
                //building with numbers, when you press 1-9, 0 and minus it builds the block you wany and highlights it
                if (buildMode == true)
                {
                    if (e.KeyCode == Keys.D1)
                    {
                        block = 6;
                        picSnowSelect.BackColor = Color.Black;
                        picWoodSelect.BackColor = Color.Black;
                        picSandSelect.BackColor = Color.Black;
                        picGravelSelect.BackColor = Color.Black;
                        picGrassSelect.BackColor = Color.DarkGray;
                        picSandStoneSelect.BackColor = Color.Black;
                        picIceSelect.BackColor = Color.Black;
                        picStoneSelect.BackColor = Color.Black;
                        picBrickSelect.BackColor = Color.Black;
                        picDelete.BackColor = Color.Black;
                        picMudSelect.BackColor = Color.Black;

                    }
                    else if (e.KeyCode == Keys.D2)
                    {
                        block = 2;
                        picSnowSelect.BackColor = Color.Black;
                        picWoodSelect.BackColor = Color.Black;
                        picSandSelect.BackColor = Color.Black;
                        picGravelSelect.BackColor = Color.Black;
                        picGrassSelect.BackColor = Color.Black;
                        picSandStoneSelect.BackColor = Color.Black;
                        picIceSelect.BackColor = Color.Black;
                        picStoneSelect.BackColor = Color.Black;
                        picBrickSelect.BackColor = Color.Black;
                        picDelete.BackColor = Color.Black;
                        picMudSelect.BackColor = Color.DarkGray;
                    }
                    else if (e.KeyCode == Keys.D3)
                    {
                        block = 1;
                        picSnowSelect.BackColor = Color.DarkGray;
                        picWoodSelect.BackColor = Color.Black;
                        picSandSelect.BackColor = Color.Black;
                        picGravelSelect.BackColor = Color.Black;
                        picGrassSelect.BackColor = Color.Black;
                        picSandStoneSelect.BackColor = Color.Black;
                        picIceSelect.BackColor = Color.Black;
                        picStoneSelect.BackColor = Color.Black;
                        picBrickSelect.BackColor = Color.Black;
                        picDelete.BackColor = Color.Black;
                        picMudSelect.BackColor = Color.Black;
                    }
                    else if (e.KeyCode == Keys.D4)
                    {
                        block = 7;
                        picSnowSelect.BackColor = Color.Black;
                        picWoodSelect.BackColor = Color.Black;
                        picSandSelect.BackColor = Color.Black;
                        picGravelSelect.BackColor = Color.Black;
                        picGrassSelect.BackColor = Color.Black;
                        picSandStoneSelect.BackColor = Color.Black;
                        picIceSelect.BackColor = Color.DarkGray;
                        picStoneSelect.BackColor = Color.Black;
                        picBrickSelect.BackColor = Color.Black;
                        picDelete.BackColor = Color.Black;
                        picMudSelect.BackColor = Color.Black;
                    }
                    else if (e.KeyCode == Keys.D5)
                    {
                        block = 5;
                        picSnowSelect.BackColor = Color.Black;
                        picWoodSelect.BackColor = Color.Black;
                        picSandSelect.BackColor = Color.DarkGray;
                        picGravelSelect.BackColor = Color.Black;
                        picGrassSelect.BackColor = Color.Black;
                        picSandStoneSelect.BackColor = Color.Black;
                        picIceSelect.BackColor = Color.Black;
                        picStoneSelect.BackColor = Color.Black;
                        picBrickSelect.BackColor = Color.Black;
                        picDelete.BackColor = Color.Black;
                        picMudSelect.BackColor = Color.Black;
                    }
                    else if (e.KeyCode == Keys.D6)
                    {
                        block = 4;
                        picSnowSelect.BackColor = Color.Black;
                        picWoodSelect.BackColor = Color.Black;
                        picSandSelect.BackColor = Color.Black;
                        picGravelSelect.BackColor = Color.Black;
                        picGrassSelect.BackColor = Color.Black;
                        picSandStoneSelect.BackColor = Color.DarkGray;
                        picIceSelect.BackColor = Color.Black;
                        picStoneSelect.BackColor = Color.Black;
                        picBrickSelect.BackColor = Color.Black;
                        picDelete.BackColor = Color.Black;
                        picMudSelect.BackColor = Color.Black;
                    }
                    else if (e.KeyCode == Keys.D7)
                    {
                        block = 3;
                        picSnowSelect.BackColor = Color.Black;
                        picWoodSelect.BackColor = Color.Black;
                        picSandSelect.BackColor = Color.Black;
                        picGravelSelect.BackColor = Color.Black;
                        picGrassSelect.BackColor = Color.Black;
                        picSandStoneSelect.BackColor = Color.Black;
                        picIceSelect.BackColor = Color.Black;
                        picStoneSelect.BackColor = Color.DarkGray;
                        picBrickSelect.BackColor = Color.Black;
                        picDelete.BackColor = Color.Black;
                        picMudSelect.BackColor = Color.Black;
                    }
                    else if (e.KeyCode == Keys.D8)
                    {
                        block = 8;
                        picSnowSelect.BackColor = Color.Black;
                        picWoodSelect.BackColor = Color.Black;
                        picSandSelect.BackColor = Color.Black;
                        picGravelSelect.BackColor = Color.DarkGray;
                        picGrassSelect.BackColor = Color.Black;
                        picSandStoneSelect.BackColor = Color.Black;
                        picIceSelect.BackColor = Color.Black;
                        picStoneSelect.BackColor = Color.Black;
                        picBrickSelect.BackColor = Color.Black;
                        picDelete.BackColor = Color.Black;
                        picMudSelect.BackColor = Color.Black;
                    }
                    else if (e.KeyCode == Keys.D9)
                    {
                        block = 9;
                        picSnowSelect.BackColor = Color.Black;
                        picWoodSelect.BackColor = Color.DarkGray;
                        picSandSelect.BackColor = Color.Black;
                        picGravelSelect.BackColor = Color.Black;
                        picGrassSelect.BackColor = Color.Black;
                        picSandStoneSelect.BackColor = Color.Black;
                        picIceSelect.BackColor = Color.Black;
                        picStoneSelect.BackColor = Color.Black;
                        picBrickSelect.BackColor = Color.Black;
                        picDelete.BackColor = Color.Black;
                        picMudSelect.BackColor = Color.Black;
                    }
                    else if (e.KeyCode == Keys.D0)
                    {
                        block = 10;
                        picSnowSelect.BackColor = Color.Black;
                        picWoodSelect.BackColor = Color.Black;
                        picSandSelect.BackColor = Color.Black;
                        picGravelSelect.BackColor = Color.Black;
                        picGrassSelect.BackColor = Color.Black;
                        picSandStoneSelect.BackColor = Color.Black;
                        picIceSelect.BackColor = Color.Black;
                        picStoneSelect.BackColor = Color.Black;
                        picBrickSelect.BackColor = Color.DarkGray;
                        picDelete.BackColor = Color.Black;
                        picMudSelect.BackColor = Color.Black;
                    }
                    else if (e.KeyCode == Keys.OemMinus)
                    {
                        block = 11;
                        picSnowSelect.BackColor = Color.Black;
                        picWoodSelect.BackColor = Color.Black;
                        picSandSelect.BackColor = Color.Black;
                        picGravelSelect.BackColor = Color.Black;
                        picGrassSelect.BackColor = Color.Black;
                        picSandStoneSelect.BackColor = Color.Black;
                        picIceSelect.BackColor = Color.Black;
                        picStoneSelect.BackColor = Color.Black;
                        picBrickSelect.BackColor = Color.Black;
                        picDelete.BackColor = Color.DarkGray;
                        picMudSelect.BackColor = Color.Black;
                    }
                    else if (e.KeyCode == Keys.P)
                    {
                        block = 12;
                    }
                    else if (e.KeyCode == Keys.V)
                    {
                        block = 13;
                    }
                }
                else
                {

                    if (e.KeyCode == Keys.K)//k to change appearance
                    {
                        //all male versions
                        if (appearance == 0 && gen == true)
                        {
                            bmpChar = new Bitmap(frmG.picGuyRags.Image);
                            bmpChar.MakeTransparent(Color.Fuchsia);
                            appearance = 1;
                        }
                        else if (appearance == 1 && gen == true)
                        {
                            bmpChar = new Bitmap(frmG.picGuyWood.Image);
                            bmpChar.MakeTransparent(Color.Fuchsia);
                            appearance = 2;
                        }
                        else if (appearance == 2 && gen == true)
                        {
                            bmpChar = new Bitmap(frmG.picGuyIron.Image);
                            bmpChar.MakeTransparent(Color.Fuchsia);
                            appearance = 3;
                        }
                        else if (appearance == 3 && gen == true)
                        {
                            bmpChar = new Bitmap(frmG.picGuyRuby.Image);
                            bmpChar.MakeTransparent(Color.Fuchsia);
                            appearance = 0;
                        }
                        //all female versions
                        else if (appearance == 0 && gen == false)
                        {
                            bmpChar = new Bitmap(frmG.picGirlRags.Image);
                            bmpChar.MakeTransparent(Color.Fuchsia);
                            appearance = 1;
                        }
                        else if (appearance == 1 && gen == false)
                        {
                            bmpChar = new Bitmap(frmG.picGirlWood.Image);
                            bmpChar.MakeTransparent(Color.Fuchsia);
                            appearance = 2;
                        }
                        else if (appearance == 2 && gen == false)
                        {
                            bmpChar = new Bitmap(frmG.picGirlIron.Image);
                            bmpChar.MakeTransparent(Color.Fuchsia);
                            appearance = 3;
                        }
                        else if (appearance == 3 && gen == false)
                        {
                            bmpChar = new Bitmap(frmG.picGirlRuby.Image);
                            bmpChar.MakeTransparent(Color.Fuchsia);
                            appearance = 0;
                        }
                    }
                }
            }
            if (control)
            {
                if (e.KeyCode == Keys.Escape && screen == 0 && torf == true)//allows ecape key to hide game and show loading screen
                {
                    picMale.Show();
                    picFemale.Show();
                    picNew.Show();
                    picLogo.Show();
                    picSelect.Show();
                    picX.Show();
                    picSave.Show();
                    picControls.Show();
                    picCreative.Show();
                    picParkour.Show();
                    picSelectGame.Show();
                    picSnowSelect.Hide();
                    picWoodSelect.Hide();
                    picSandSelect.Hide();
                    picGravelSelect.Hide();
                    picGrassSelect.Hide();
                    picSandStoneSelect.Hide();
                    picIceSelect.Hide();
                    picStoneSelect.Hide();
                    picBrickSelect.Hide();
                    picDelete.Hide();
                    picMudSelect.Hide();
                    picPressBut.Hide();
                    pic0.Hide();
                    pic1.Hide();
                    pic2.Hide();
                    pic3.Hide();
                    pic4.Hide();
                    pic5.Hide();
                    pic6.Hide();
                    pic7.Hide();
                    pic8.Hide();
                    pic9.Hide();
                    pic0.Hide();
                    picMinus.Hide();
                    picTime.Hide();
                    lblTime.Hide();
                    picSn.Show();
                    timer1.Stop();
                    screen = 1;
                    torf = false;
                }
                else if (e.KeyCode == Keys.Escape && screen == 1 && torf == false)//allows ecape key to hide loading screen and show game
                {
                    picMale.Hide();
                    picFemale.Hide();
                    picNew.Hide();
                    picLogo.Hide();
                    picSelect.Hide();
                    picX.Hide();
                    picControls.Hide();
                    picSn.Hide();
                    picCreative.Hide();
                    picParkour.Hide();
                    picSelectGame.Hide();
                    if (buildMode == true)//if buildmode was on it stays on when you press escape
                    {
                        pic0.Show();
                        pic1.Show();
                        pic2.Show();
                        pic3.Show();
                        pic4.Show();
                        pic5.Show();
                        pic6.Show();
                        pic7.Show();
                        pic8.Show();
                        pic9.Show();
                        pic0.Show();
                        picMinus.Show();
                        picSnowSelect.Show();
                        picWoodSelect.Show();
                        picSandSelect.Show();
                        picGravelSelect.Show();
                        picGrassSelect.Show();
                        picSandStoneSelect.Show();
                        picIceSelect.Show();
                        picStoneSelect.Show();
                        picBrickSelect.Show();
                        picDelete.Show();
                        picMudSelect.Show();
                    }
                    if(creative==false)//if parkour mode is on it shows timer
                    {
                        picTime.Show();
                        lblTime.Show();
                        timer1.Start();
                    }
                    picSave.Hide();
                    screen = 0;
                    torf = true;
                }
            }
            if(loading&&pressU)//pressing u when it tells you to does newgame procedure
            {
                if (e.KeyCode == Keys.U)//Press u to start game
                {
                    if (screen == 0)
                    {
                        newGame();
                    }
                }
            }
        }
        //Resets everything to start a new game.
        private void newGame()
        {
            //Establishes the bitmaps for the environment and backbuffer.
            environment = new Bitmap(map.GetLength(0) * tileSize, map.GetLength(1) * tileSize);
            backbuffer = new Bitmap(this.Width, this.Height * tileSize);
            //Depending on the game mode...
            if (creative)
            { 
                //Plays theme music for selected background.
                if(backgselect == 0)
                {
                    SoundPlayer player = new SoundPlayer();
                    player.SoundLocation = AppDomain.CurrentDomain.BaseDirectory + "\\P223Lvl1.wav";
                    player.Play();
                }
                else if(backgselect == 1)
                {
                    SoundPlayer player = new SoundPlayer();
                    player.SoundLocation = AppDomain.CurrentDomain.BaseDirectory + "\\P223MainTheme.wav";
                    player.Play();
                }
                else if(backgselect == 2)
                {
                    SoundPlayer player = new SoundPlayer();
                    player.SoundLocation = AppDomain.CurrentDomain.BaseDirectory + "\\p223Des.wav";
                    player.Play();
                }
                else if (backgselect == 3)
                {
                    SoundPlayer player = new SoundPlayer();
                    player.SoundLocation = AppDomain.CurrentDomain.BaseDirectory + "\\p223Night.wav";
                    player.Play();
                }
                Graphics gEnvironment = Graphics.FromImage(environment);
                Graphics g = this.CreateGraphics();
                Graphics gback = Graphics.FromImage(backbuffer);                                                                                              //Everything is drawn onto the backbuffer so everything can be drawn on the form at once.
                gback.DrawImage(environment, cornerX, cornerY, environment.Width, environment.Height);                                                        //Draws the map starting at the top left corner coordinates.
                rectFrame = new Rectangle(rectPlayerPos.Width * movementFrame, rectPlayerPos.Height * direction, rectPlayerPos.Width, rectPlayerPos.Height);  //Determines which frame of the player to draw.
                gback.DrawImage(bmpChar, rectPlayerPos, rectFrame, GraphicsUnit.Pixel);                                                                        //Draws player frame at the player position.
                //Prepares creative file to be read.
                string filePath = Application.StartupPath + "\\creative.txt";
                FileStream fs = new FileStream(filePath, FileMode.Open);
                StreamReader myFile = new StreamReader(fs);
                //Draws environment tiles according to info in file.
                for (int y = 0; y < map.GetLength(1); y++)
                {
                    for (int x = 0; x < map.GetLength(0); x++)
                    {
                        map[x, y] = int.Parse(myFile.ReadLine());
                        //Check the element value and draws the corresponding tile on the environment bitmap at its corresponding coordinates.
                        if (x == 0 || y == 0 || x == map.GetLength(0) - 1 || y == map.GetLength(1) - 1)
                        {
                            gEnvironment.DrawImage(bmpBare, x * tileSize, y * tileSize, tileSize, tileSize);
                        }
                        else if (map[x, y] == 1)
                        {
                            gEnvironment.DrawImage(bmpSnow, x * tileSize, y * tileSize, tileSize, tileSize);
                        }
                        else if (map[x, y] == 2)
                        {
                            gEnvironment.DrawImage(bmpMud, x * tileSize, y * tileSize, tileSize, tileSize);
                        }
                        else if (map[x, y] == 3)
                        {
                            gEnvironment.DrawImage(bmpStone, x * tileSize, y * tileSize, tileSize, tileSize);
                        }
                        else if (map[x, y] == 4)
                        {
                            gEnvironment.DrawImage(bmpsand, x * tileSize, y * tileSize, tileSize, tileSize);
                        }
                        else if (map[x, y] == 5)
                        {
                            gEnvironment.DrawImage(bmpSandStone, x * tileSize, y * tileSize, tileSize, tileSize);
                        }
                        else if (map[x, y] == 6)
                        {
                            gEnvironment.DrawImage(bmpGrass, x * tileSize, y * tileSize, tileSize, tileSize);
                        }
                        else if (map[x, y] == 7)
                        {
                            gEnvironment.DrawImage(bmpice, x * tileSize, y * tileSize, tileSize, tileSize);
                        }
                        else if (map[x, y] == 8)
                        {
                            gEnvironment.DrawImage(bmpGravel, x * tileSize, y * tileSize, tileSize, tileSize);
                        }
                        else if (map[x, y] == 0)
                        {
                            gEnvironment.DrawImage(bmpAir, mouseX, mouseY, tileSize, tileSize);
                        }
                        else if (map[x, y] == 9)
                        {
                            gEnvironment.DrawImage(bmpWood, x * tileSize, y * tileSize, tileSize, tileSize);
                        }
                        else if (map[x, y] == 10)
                        {
                            gEnvironment.DrawImage(bmpBrick, x * tileSize, y * tileSize, tileSize, tileSize);
                        }
                        else if (map[x, y] == 12)
                        {
                            gEnvironment.DrawImage(bmpTrophy, x * tileSize, y * tileSize, tileSize, tileSize);
                        }
                        else if (map[x, y] == 13)
                        {
                            gEnvironment.DrawImage(bmpLava, x * tileSize, y * tileSize, tileSize, tileSize);
                        }
                    }
                }
                environment.MakeTransparent(Color.Fuchsia);
                myFile.Close();
                gback.Dispose();
                g.DrawImage(backbuffer, 0, 0);                                                                                                                //Draws backbuffer onto form.
                g.Dispose();
                //Spawn point is at top middle of map.
                cornerX = map.GetLength(0) / 2 * -tileSize;
                cornerY = 0;
            }
            else if (creative == false)
            {
                SoundPlayer player = new SoundPlayer();
                player.SoundLocation = AppDomain.CurrentDomain.BaseDirectory + "\\P223FirstBoss(Complete).wav";
                player.Play();
                Graphics gEnvironment = Graphics.FromImage(environment);
                Graphics g = this.CreateGraphics();
                Graphics gback = Graphics.FromImage(backbuffer);                                                                                              //Everything is drawn onto the backbuffer so everything can be drawn on the form at once.
                rectFrame = new Rectangle(rectPlayerPos.Width * movementFrame, rectPlayerPos.Height * direction, rectPlayerPos.Width, rectPlayerPos.Height);  //Determines which frame of the player to draw.
                gback.DrawImage(bmpChar, rectPlayerPos, rectFrame, GraphicsUnit.Pixel);                                                                        //Draws player frame at the player position.
                //Prepares parkour file to be read.
                string filePath = Application.StartupPath + "\\parkour.txt";
                FileStream fs = new FileStream(filePath, FileMode.Open);
                StreamReader myFile = new StreamReader(fs);
                //Draws environment tiles according to info in file.
                for (int y = 0; y < map.GetLength(1); y++)
                {
                    for (int x = 0; x < map.GetLength(0); x++)
                    {
                        map[x, y] = int.Parse(myFile.ReadLine());
                        //Check the element value and draws the corresponding tile on the environment bitmap at its corresponding coordinates.
                        if (x == 0 || y == 0 || x == map.GetLength(0) - 1 || y == map.GetLength(1) - 1)
                        {
                            gEnvironment.DrawImage(bmpBare, x * tileSize, y * tileSize, tileSize, tileSize);
                        }
                        else if (map[x, y] == 1)
                        {
                            gEnvironment.DrawImage(bmpSnow, x * tileSize, y * tileSize, tileSize, tileSize);
                        }
                        else if (map[x, y] == 2)
                        {
                            gEnvironment.DrawImage(bmpMud, x * tileSize, y * tileSize, tileSize, tileSize);
                        }
                        else if (map[x, y] == 3)
                        {
                            gEnvironment.DrawImage(bmpStone, x * tileSize, y * tileSize, tileSize, tileSize);
                        }
                        else if (map[x, y] == 4)
                        {
                            gEnvironment.DrawImage(bmpsand, x * tileSize, y * tileSize, tileSize, tileSize);
                        }
                        else if (map[x, y] == 5)
                        {
                            gEnvironment.DrawImage(bmpSandStone, x * tileSize, y * tileSize, tileSize, tileSize);
                        }
                        else if (map[x, y] == 6)
                        {
                            gEnvironment.DrawImage(bmpGrass, x * tileSize, y * tileSize, tileSize, tileSize);
                        }
                        else if (map[x, y] == 7)
                        {
                            gEnvironment.DrawImage(bmpice, x * tileSize, y * tileSize, tileSize, tileSize);
                        }
                        else if (map[x, y] == 8)
                        {
                            gEnvironment.DrawImage(bmpGravel, x * tileSize, y * tileSize, tileSize, tileSize);
                        }
                        else if (map[x, y] == 0)
                        {
                            gEnvironment.DrawImage(bmpAir, mouseX, mouseY, tileSize, tileSize);
                        }
                        else if (map[x, y] == 9)
                        {
                            gEnvironment.DrawImage(bmpWood, x * tileSize, y * tileSize, tileSize, tileSize);
                        }
                        else if (map[x, y] == 10)
                        {
                            gEnvironment.DrawImage(bmpBrick, x * tileSize, y * tileSize, tileSize, tileSize);
                        }
                        else if (map[x, y] == 12)
                        {
                            gEnvironment.DrawImage(bmpTrophy, x * tileSize, y * tileSize, tileSize, tileSize);
                        }
                        else if (map[x, y] == 13)
                        {
                            gEnvironment.DrawImage(bmpLava, x * tileSize, y * tileSize, tileSize, tileSize);
                        }
                    }
                }
                myFile.Close();
                gback.Dispose();
                environment.MakeTransparent(Color.Fuchsia);
                g.DrawImage(backbuffer, 0, 0);                                                                                                                //Draws backbuffer onto form.
                g.Dispose();
                //Spawn point for parkour map.
                cornerX = map.GetLength(0) / 5 * -tileSize;
                cornerY = (map.GetLength(1) - 12) * -tileSize;
            }
            //Sets clock timer to zero.
            minutes = 0;
            seconds = 0;
            picStart.Hide();
            picSn.Hide();
            picPressBut.Show();
            if (creative == false)
            {
                timer1.Start();
                picTime.Show();
                lblTime.Show();
                picPressBut.Hide();
            }
            torf = true;
            moveTimer.Enabled = true;
            pressU = false;
            bothclicked = false;
            jumped = false;
            moving = false;
        }
    }
}
