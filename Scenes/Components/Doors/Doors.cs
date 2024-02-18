using Godot;
using System;

public class Doors : Node2D
{
    // Declare member variables here.

    //Load door scene
    private PackedScene Door = GD.Load("res://Scenes/Sprites/Door.tscn") as PackedScene;
    private Node2D[] DoorsList = new Node2D[3];
    private const int AmountOfDoors = 3;
    private Node2D DoorsContainer = new Node2D();
    private RandomNumberGenerator Rng = new RandomNumberGenerator();
    private Label WinsLabel = new Label();
    private Label LossesLabel = new Label();
    private Label WinPercentLabel = new Label();
    private Label[] PercentDoorLabels = new Label[3];
    private Sprite InstructionsMenuSprite;
    private Button InstructionMenuButton;
    private Sprite ExplanationMenuSprite;
    private Button ExplanationMenuButton;

    private int WinsCount = 0;
    private int LossesCount = 0;
    private float WinPercent = 0.0f;

    // private Label TimerTestLabel = new Label();

    private Timer ResetGameTimer = new Timer();


    private string GameStage = "Start";

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        //Target doors container node to place doors in
        DoorsContainer = GetNode("DoorsContainer") as Node2D;
        ResetGameTimer = GetNode("ResetGameTimer") as Timer;

        InstructionsMenuSprite = GetNode("InstructionsMenuSprite") as Sprite;
        InstructionMenuButton = GetNode("InstructionsMenuButton") as Button;
        ExplanationMenuSprite = GetNode("ExplanationMenuSprite") as Sprite;
        ExplanationMenuButton = GetNode("ExplanationMenuButton") as Button;

        //Find label node
        WinsLabel = GetNode("WinsLabel") as Label;
        WinsLabel.Text = $"Wins : {WinsCount}";

        LossesLabel = GetNode("LossesLabel") as Label;
        LossesLabel.Text = $"Losses : {LossesCount}";

        WinPercentLabel = GetNode("WinPercentLabel") as Label;
        WinPercentLabel.Text = $"Win Rate : {WinPercent}%";

        PercentDoorLabels[0] = GetNode("PercentDoor1Label") as Label;
        PercentDoorLabels[0].Text = "33%";

        PercentDoorLabels[1] = GetNode("PercentDoor2Label") as Label;
        PercentDoorLabels[1].Text = "33%";

        PercentDoorLabels[2] = GetNode("PercentDoor3Label") as Label;
        PercentDoorLabels[2].Text = "33%";

        //Timer test label
        // TimerTestLabel = GetNode("TimerTestLabel") as Label;

        //Generate a new seed everytime doors scene loads
        Rng.Randomize();

        CreateGame();
    }

    public void CreateGame()
    {
        //Door positions
        int DoorPosX = 37;
        int DoorPosY = 205;

        //Choose which door will be correct
        int correctDoor = Rng.RandiRange(0, 2);

        //Generate doors
        for (int i = 0; i < AmountOfDoors; i++)
        {
            //Create door instances
            DoorsList[i] = Door.Instance() as Node2D;

            //Set door positions
            DoorsList[i].Position = new Vector2(DoorPosX, DoorPosY);

            //Set each doors state (correct or wrong)
            if (i != correctDoor)
            {
                DoorsList[i].Call("SetDoorState", "OpenWrong");
            }
            else if (i == correctDoor)
            {
                DoorsList[i].Call("SetDoorState", "OpenCorrect");
            }
            else
            {
                Console.WriteLine("Error in setting door state in generate doors");
            }

            //Add doors to scene
            DoorsContainer.AddChild(DoorsList[i]);

            //Increment door position
            DoorPosX += 350;
        }
    }

    public void ResetDoors()
    {
        //Get each child in the doors container node and delete them all
        for (int i = 0; i < DoorsContainer.GetChildCount(); i++)
        {
            DoorsContainer.GetChild(i).QueueFree();
            PercentDoorLabels[i].Text = "33%";
        }
    }

    //Handles game logic
    //This function is called from Door.cs via a signal
    public void DoorPressed(Node2D door)
    {
        //Doors sprite
        var doorSprite = door.Get("doorSprite") as AnimatedSprite;

        //Checks if door is a valid target in its current state, only closed doors should be clickable
        if (doorSprite.Animation == "OpenCorrect" || doorSprite.Animation == "OpenWrong")
        {
            return;
        }

        //A selected door should only be clickable during the playerpicksagain stage
        if (doorSprite.Animation == "ClosedSelected" && GameStage != "PlayerPicksAgain")
        {
            return;
        }

        //Nothing should be clickable once the game is finished, a timer will reset the game automatically
        if (GameStage == "GameFinished")
        {
            return;
        }


        if (GameStage == "Start")
        {
            door.Set("DoorPicked", true);
            door.Call("SetDoorAnimation", "ClosedSelected");

            OpenIncorrectDoor(door);

            SetDoorPercentages();

            GameStage = "PlayerPicksAgain";
            return;
        }

        if (GameStage == "PlayerPicksAgain")
        {
            door.Call("OpenDoor", true);

            GameStage = "GameFinished";
            if ((string)door.Get("DoorState") == "OpenCorrect")
            {
                WinsCount++;
                WinsLabel.Text = $"Wins :{WinsCount}";
            }
            else
            {
                LossesCount++;
                LossesLabel.Text = $"Losses :{LossesCount}";
            }

            int totalGames = WinsCount + LossesCount;
            WinPercent = ((float)WinsCount / totalGames) * 100;
            WinPercentLabel.Text = $"Win Rate :{WinPercent}%";

            // var color = new Color(0,0,0);
            // if (WinPercent > 50f) {
            //     color = new Color(0,255,0);
            // } else {
            //     color = new Color(255,0,0);
            // }
            // WinPercentLabel.AddColorOverride("font_color", color);


            //Countdown to reset the game
            ResetGameTimer.Start();
            return;
        }

        //Add a state check to this if statement, make sure its the final stage

    }

    //Pick an incorrect door to open, after the player chooses their door
    public void OpenIncorrectDoor(Node2D door)
    {
        //Start the search from either the left or the right, so its not obvious which door is correct
        int startLeftOrRight = Rng.RandiRange(0, 1);

        if (startLeftOrRight == 0) //Start left
        {
            for (int i = 0; i < AmountOfDoors; i++)
            {
                if ((string)DoorsList[i].Get("DoorState") == "OpenWrong" && !(bool)DoorsList[i].Get("DoorPicked"))
                {
                    DoorsList[i].Call("OpenDoor", false);
                    break;
                }
            }
        }
        else if (startLeftOrRight == 1) //Start right
        {
            for (int i = AmountOfDoors - 1; i >= 0; i--)
            {
                if ((string)DoorsList[i].Get("DoorState") == "OpenWrong" && !(bool)DoorsList[i].Get("DoorPicked"))
                {
                    DoorsList[i].Call("OpenDoor", false);
                    break;
                }
            }
        }
    }

    //Set the door percentages after player makes first pick
    public void SetDoorPercentages()
    {
        for (int i = 0; i < AmountOfDoors; i++)
        {
            var DoorSprite = DoorsList[i].Get("doorSprite") as AnimatedSprite;
            Console.WriteLine("DOOR SPRITE = " + DoorSprite.Animation);

            if (DoorSprite.Animation == "OpenWrong")
            {
                PercentDoorLabels[i].Text = "0%";
            }
            else if (DoorSprite.Animation == "Closed")
            {
                PercentDoorLabels[i].Text = "66%";
            }

        }
    }

    public void DisplayDoorPercentButtonPressed()
    {
        for (int i = 0; i < 3; i++)
        {
            PercentDoorLabels[i].Visible = !PercentDoorLabels[i].Visible;
        }
    }

    public void ResetWinLossButtonPressed()
    {
        WinsCount = 0;
        LossesCount = 0;
        WinsLabel.Text = $"Wins : {WinsCount}";
        LossesLabel.Text = $"Losses : {LossesCount}";
        WinPercentLabel.Text = "Win Rate : 0%";
    }

    public void ExplanationMenuButtonPressed() {
        ExplanationMenuSprite.Visible = !ExplanationMenuSprite.Visible;
        if (ExplanationMenuSprite.Visible) {
            InstructionMenuButton.Visible = false;
        } else {
            InstructionMenuButton.Visible = true;
        }
    }
    public void InstructionsMenuButtonPressed() {
        InstructionsMenuSprite.Visible = !InstructionsMenuSprite.Visible;
        if (InstructionsMenuSprite.Visible) {
            InstructionMenuButton.Text = "CLOSE";
            ExplanationMenuButton.Visible = false;
        } else {
            InstructionMenuButton.Text = "INSTRUCTIONS";
            ExplanationMenuButton.Visible = true;
        }
    }
    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)

    {
        // TimerTestLabel.Text = ResetGameTimer.TimeLeft.ToString();

        if (GameStage == "GameFinished" && ResetGameTimer.TimeLeft <= 0)
        {
            Console.WriteLine("GAME RESET");
            GameStage = "Start";
            ResetDoors();
            CreateGame();
        }
    }


}


