using Godot;
using System;

public class Door : Node2D
{
    // Declare member variables here.
    private AnimatedSprite doorSprite;
    private AudioStreamPlayer2D DoorSound;
    private AudioStreamPlayer2D MonsterSound;
    private AudioStreamPlayer2D WinSound;

    private string DoorState { get; set; } = "NULL";

    private bool DoorPicked = false;

    [Signal]
    public delegate void DoorPressed();

    private Node DoorsScene;

    public void SetDoorState(string state)
    {
        DoorState = state;
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        //Get doors scene, probably a terrible way to do it
        DoorsScene = this.GetParent();
        DoorsScene = DoorsScene.GetParent();
        this.Connect("DoorPressed", DoorsScene, "DoorPressed");

        doorSprite = GetNode("DoorSprite") as AnimatedSprite;
        DoorSound = GetNode("DoorSound") as AudioStreamPlayer2D;
        MonsterSound = GetNode("MonsterSound") as AudioStreamPlayer2D;
        WinSound = GetNode("WinSound") as AudioStreamPlayer2D;
    }

    public void OnDoorButtonPressed()
    {
        //Send signal to Doors scene that a button has been pressed
        EmitSignal("DoorPressed", this);
    }

    public void OpenDoor(bool playSound)
    {
        doorSprite.Play(DoorState);
        if (DoorState == "OpenCorrect")
        {
            // DoorSound.Play();
            WinSound.Play();
        } else if (DoorState == "OpenWrong" && playSound == true){
            MonsterSound.Play();
        }

    }

    public void SetDoorAnimation(string animationName)
    {
        doorSprite.Play(animationName);
    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {

    }
}
