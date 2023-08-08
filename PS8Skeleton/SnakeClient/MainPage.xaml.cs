using System.Net.Sockets;
using System.Net;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Model;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace SnakeGame;

public partial class MainPage : ContentPage
{
    //private SocketState theServer;
    //private World theWorld;

    GameController controller = new();
    public MainPage()
    {
        InitializeComponent();

        //Subscribe to events that the GameControll can use.
        controller.UpdateArrived += OnFrame;
        controller.Error += NetworkErrorHandler;
        controller.CreateWorld += CreateNewWorld;
    }

    //A hack to keep the input box highlighted while controlling the snake.
    void OnTapped(object sender, EventArgs args)
    {
        keyboardHack.Focus();
    }

    //A hack that tells the controller an input has been entered, either up/down/left/right.
    void OnTextChanged(object sender, TextChangedEventArgs args)
    {
        Entry entry = (Entry)sender;
        String text = entry.Text.ToLower();
        if (text == "w")
        {
            controller.inputEntered("{\"moving\":\"up\"}\n");
        }
        else if (text == "a")
        {
            controller.inputEntered("{\"moving\":\"left\"}\n");
        }
        else if (text == "s")
        {
            controller.inputEntered("{\"moving\":\"down\"}\n");
        }
        else if (text == "d")
        {
            controller.inputEntered("{\"moving\":\"right\"}\n");
        }
        entry.Text = "";
    }

    /// <summary>
    /// Displays a given alert when an error occurs.
    /// </summary>
    /// <param name="err"></param>
    private void NetworkErrorHandler(string err)
    {
        Dispatcher.Dispatch(() =>
        {
            DisplayAlert("Error", err, "OK");
            connectButton.IsEnabled = true;
            serverText.IsEnabled = true;
            controller.CloseWorld();
        });
    }


    /// <summary>
    /// Event handler for the connect button
    /// We will put the connection attempt logic here in the view, instead of the controller,
    /// because it is closely tied with disabling/enabling buttons, and showing dialogs.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void ConnectClick(object sender, EventArgs args)
    {
        if (serverText.Text == "")
        {
            DisplayAlert("Error", "Please enter a server address", "OK");
            return;
        }
        if (nameText.Text == "")
        {
            DisplayAlert("Error", "Please enter a name", "OK");
            return;
        }
        if (nameText.Text.Length > 16)
        {
            DisplayAlert("Error", "Name must be less than 16 characters", "OK");
            return;
        }

        Dispatcher.Dispatch(() => connectButton.IsEnabled = false);
        serverText.IsEnabled = false;
        keyboardHack.Focus();
        controller.Connect(serverText.Text, nameText.Text);
    }


    /// <summary>
    /// Displays the controlls when the controlls button is clicked.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ControlsButton_Clicked(object sender, EventArgs e)
    {
        DisplayAlert("Controls",
                     "W:\t\t Move up\n" +
                     "A:\t\t Move left\n" +
                     "S:\t\t Move down\n" +
                     "D:\t\t Move right\n",
                     "OK");
    }

    /// <summary>
    /// Displays the about section when clicked.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void AboutButton_Clicked(object sender, EventArgs e)
    {
        DisplayAlert("About",
      "SnakeGame solution\nArtwork by Jolie Uk and Alex Smith\nGame design by Daniel Kopta and Travis Martin\n" +
      "Implementation by Alex Cao and Lucas Jones\n" +
        "CS 3500 Fall 2022, University of Utah", "OK");
    }

    //A hack to use the input box as a controll stick for the snake.
    private void ContentPage_Focused(object sender, FocusEventArgs e)
    {
        if (!connectButton.IsEnabled)
            keyboardHack.Focus();
    }

    //Invalidates the graphics view every frame.
    private void OnFrame()
    {
        Dispatcher.Dispatch(() => graphicsView.Invalidate());
    }

    //Creates a world panel that uses the controller.
    private void CreateNewWorld()
    {
        worldPanel.CreateFromWorld(controller);
    }
}