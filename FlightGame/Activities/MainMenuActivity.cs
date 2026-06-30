using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FlightGame.Activities;

public class MainMenuActivity : IActivity
{
    private readonly MenuItem[] _menuItems =
    [
        new("Testing", () => new TestingActivity()),
        new("Landscape Designer", () => new LandscapeDesignerActivity()),
        new("Options", () => new OptionsActivity()),
        new("Quit", () => new QuitActivity()),
    ];

    private IActivityHost? _host;
    private ActivityContext? _context;
    private int _selectedIndex;
    private KeyboardState _previousKeyboardState;
    private MouseState _previousMouseState;

    public void Enter(IActivityHost host, ActivityContext context)
    {
        _host = host;
        _context = context;
        _selectedIndex = 0;
        _previousKeyboardState = Keyboard.GetState();
        _previousMouseState = Mouse.GetState();
    }

    public void Exit()
    {
        _host = null;
        _context = null;
    }

    public void Update(GameTime gameTime)
    {
        var keyboardState = Keyboard.GetState();
        var mouseState = Mouse.GetState();

        if (IsKeyPressed(keyboardState, Keys.Up))
        {
            _selectedIndex = (_selectedIndex - 1 + _menuItems.Length) % _menuItems.Length;
        }
        else if (IsKeyPressed(keyboardState, Keys.Down))
        {
            _selectedIndex = (_selectedIndex + 1) % _menuItems.Length;
        }
        else if (IsKeyPressed(keyboardState, Keys.Enter))
        {
            ActivateSelectedItem();
        }
        else if (IsKeyPressed(keyboardState, Keys.Escape))
        {
            _host?.ExitGame();
        }

        if (mouseState.LeftButton == ButtonState.Pressed &&
            _previousMouseState.LeftButton == ButtonState.Released)
        {
            var clickedIndex = GetMenuItemIndexAt(mouseState.X, mouseState.Y);
            if (clickedIndex >= 0)
            {
                _selectedIndex = clickedIndex;
                ActivateSelectedItem();
            }
        }
        else
        {
            var hoveredIndex = GetMenuItemIndexAt(mouseState.X, mouseState.Y);
            if (hoveredIndex >= 0)
            {
                _selectedIndex = hoveredIndex;
            }
        }

        _previousKeyboardState = keyboardState;
        _previousMouseState = mouseState;
    }

    public void Draw(GameTime gameTime)
    {
        ArgumentNullException.ThrowIfNull(_context);

        var device = _context.GraphicsDevice;
        device.Clear(Color.Black);

        _context.SpriteBatch.Begin();

        var title = "FlightGame";
        var titleSize = _context.Font.MeasureString(title);
        var titlePosition = new Vector2(
            (device.Viewport.Width - titleSize.X) / 2f,
            device.Viewport.Height * 0.2f);
        _context.SpriteBatch.DrawString(_context.Font, title, titlePosition, Color.White);

        var lineHeight = _context.Font.LineSpacing + 12f;
        var menuStartY = device.Viewport.Height * 0.4f;

        for (var i = 0; i < _menuItems.Length; i++)
        {
            var label = _menuItems[i].Label;
            var textSize = _context.Font.MeasureString(label);
            var position = new Vector2(
                (device.Viewport.Width - textSize.X) / 2f,
                menuStartY + i * lineHeight);
            var color = i == _selectedIndex ? Color.Yellow : Color.Gray;

            _context.SpriteBatch.DrawString(_context.Font, label, position, color);
        }

        _context.SpriteBatch.End();
    }

    private void ActivateSelectedItem()
    {
        _host?.SetActivity(_menuItems[_selectedIndex].CreateActivity());
    }

    private int GetMenuItemIndexAt(int mouseX, int mouseY)
    {
        ArgumentNullException.ThrowIfNull(_context);

        var device = _context.GraphicsDevice;
        var lineHeight = _context.Font.LineSpacing + 12f;
        var menuStartY = device.Viewport.Height * 0.4f;

        for (var i = 0; i < _menuItems.Length; i++)
        {
            var label = _menuItems[i].Label;
            var textSize = _context.Font.MeasureString(label);
            var position = new Vector2(
                (device.Viewport.Width - textSize.X) / 2f,
                menuStartY + i * lineHeight);

            var bounds = new Rectangle(
                (int)position.X,
                (int)position.Y,
                (int)textSize.X,
                (int)textSize.Y);

            if (bounds.Contains(mouseX, mouseY))
            {
                return i;
            }
        }

        return -1;
    }

    private bool IsKeyPressed(KeyboardState currentState, Keys key)
    {
        return currentState.IsKeyDown(key) && _previousKeyboardState.IsKeyUp(key);
    }

    private sealed record MenuItem(string Label, Func<IActivity> CreateActivity);
}
