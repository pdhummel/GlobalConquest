using System.Windows;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myra;
using Myra.Graphics2D;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.TextureAtlases;
using Myra.Graphics2D.UI;
using Thickness = Myra.Graphics2D.Thickness;

namespace GlobalConquest;

public class MainGameScreen
{
    Game game;
    Grid grid;

    public Panel mapPanel { get; } = new Panel();
    public Panel factionsPanel { get; } = new Panel();
    public Panel miniMapPanel { get; } = new Panel();
    public Panel detailsPanel { get; } = new Panel();
    public MainGameScreen(Game game, Grid grid)
    {
        this.game = game;
        this.grid = grid;
        ((GlobalConquestGame)game).MainGameScreen = this;
    }

    public void LoadContent()
    {
        mapPanel.Width = 800;
        mapPanel.Height = 750;
        mapPanel.Border = new SolidBrush("#808000FF");
        mapPanel.BorderThickness = new Thickness(2);

        factionsPanel.Width = 200;
        factionsPanel.Height = 250;
        factionsPanel.Border = new SolidBrush("#808000FF");
        factionsPanel.BorderThickness = new Thickness(2);

        miniMapPanel.Width = 200;
        miniMapPanel.Height = 250;
        miniMapPanel.Border = new SolidBrush("#808000FF");
        miniMapPanel.BorderThickness = new Thickness(2);

        detailsPanel.Width = 200;
        detailsPanel.Height = 250;
        detailsPanel.Border = new SolidBrush("#808000FF");
        detailsPanel.BorderThickness = new Thickness(2);

    }

    public void show()
    {
        mapPanel.Left = 0;
        mapPanel.Top = 0;
        //Grid.SetColumn(mapPanel, 0);
        //Grid.SetRow(mapPanel, 0);
        //Grid.SetRowSpan(mapPanel, 3);
        //Grid.SetColumnSpan(mapPanel, 4);
        grid.Desktop.Widgets.Add(mapPanel);
        mapPanel.Visible = true;

        //Grid.SetColumn(factionsPanel, 5);
        //Grid.SetRow(factionsPanel, 0);
        grid.Desktop.Widgets.Add(factionsPanel);
        factionsPanel.Left = 800;
        factionsPanel.Top = 0;
        factionsPanel.Visible = true;

        //Grid.SetColumn(miniMapPanel, 1);
        //Grid.SetRow(miniMapPanel, 1);
        grid.Desktop.Widgets.Add(miniMapPanel);
        miniMapPanel.Left = 800;
        miniMapPanel.Top = 250;
        miniMapPanel.Visible = true;

        //Grid.SetColumn(detailsPanel, 1);
        //Grid.SetRow(detailsPanel, 2);
        grid.Desktop.Widgets.Add(detailsPanel);
        detailsPanel.Left = 800;
        detailsPanel.Top = 500;
        detailsPanel.Visible = true;

    }

    public void hide()
    {
        mapPanel.Visible = false;
        factionsPanel.Visible = false;
        miniMapPanel.Visible = false;
        detailsPanel.Visible = false;

        mapPanel.RemoveFromParent();
        factionsPanel.RemoveFromParent();
        miniMapPanel.RemoveFromParent();
        detailsPanel.RemoveFromParent();

    }

}