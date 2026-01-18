using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static GlobalConquest.Burbs;
using static GlobalConquest.Map;
using static GlobalConquest.Resource;
using static GameConstants;

namespace GlobalConquest;

public class Textures
{
    // <a href="https://www.flaticon.com/free-icons/war" title="war icons">War icons created by Good Ware - Flaticon</a>
    // <a href="https://www.flaticon.com/free-icons/dove" title="dove icons">Dove icons created by Freepik - Flaticon</a>
    // <a href="https://www.flaticon.com/free-icons/alliance" title="alliance icons">Alliance icons created by HAJICON - Flaticon</a>
    // <a href="https://www.flaticon.com/free-icons/partner" title="partner icons">Partner icons created by Buandesign - Flaticon</a>
    // <a href="https://www.flaticon.com/free-icons/marriage" title="marriage icons">Marriage icons created by Freepik - Flaticon</a>
    // <a href="https://www.flaticon.com/free-icons/well" title="well icons">Well icons created by Aziz Muttaqin - Flaticon</a>
    // <a href="https://www.flaticon.com/free-icons/gold" title="gold icons">Gold icons created by Freepik - Flaticon</a>

    public Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();
    public Dictionary<string, Texture2D> units = new Dictionary<string, Texture2D>();
    public Dictionary<string, Texture2D> burbs = new Dictionary<string, Texture2D>();

    public void LoadContent(Game game)
    {
        Texture2D magentaMetro = game.Content.Load<Texture2D>("magenta-metro-72x72");
        burbs["magenta-metro"] = magentaMetro;
        textures["magenta-metro"] = magentaMetro;
        Texture2D amberMetro = game.Content.Load<Texture2D>("amber-metro-72x72");
        burbs["amber-metro"] = amberMetro;
        textures["amber-metro"] = amberMetro;
        Texture2D ocherMetro = game.Content.Load<Texture2D>("ocher-metro-72x72");
        burbs["ocher-metro"] = ocherMetro;
        textures["ocher-metro"] = ocherMetro;
        Texture2D cyanMetro = game.Content.Load<Texture2D>("cyan-metro-72x72");
        burbs["cyan-metro"] = cyanMetro;
        textures["cyan-metro"] = cyanMetro;
        Texture2D capitalTile = game.Content.Load<Texture2D>("capital-72x72");
        burbs[BURB_CAPITAL] = capitalTile;
        textures[BURB_CAPITAL] = capitalTile;
        Texture2D cityTile = game.Content.Load<Texture2D>("city-hex-72x72");
        burbs[BURB_CITY] = cityTile;
        textures[BURB_CITY] = cityTile;
        Texture2D townTile = game.Content.Load<Texture2D>("town-hex-72x72");
        burbs[BURB_TOWN] = townTile;
        textures[BURB_TOWN] = townTile;
        Texture2D villageTile = game.Content.Load<Texture2D>("village-hex-72x72");
        burbs[BURB_VILLAGE] = villageTile;
        textures[BURB_VILLAGE] = villageTile;

        Texture2D flameTexture = game.Content.Load<Texture2D>("flame-30px");
        textures["flame"] = flameTexture;
        Texture2D northArrowTexture = game.Content.Load<Texture2D>("north-arrow-white-72");
        textures["north-arrow"] = northArrowTexture;
        Texture2D southArrowTexture = game.Content.Load<Texture2D>("south-arrow-white-72");
        textures["south-arrow"] = southArrowTexture;

        Texture2D southTabTexture = game.Content.Load<Texture2D>("south-tab-white");
        textures["south-tab-white"] = southTabTexture;
        Texture2D southTabMagentaTexture = game.Content.Load<Texture2D>("south-tab-magenta");
        textures["south-tab-magenta"] = southTabMagentaTexture;
        Texture2D southTabCyanTexture = game.Content.Load<Texture2D>("south-tab-cyan");
        textures["south-tab-cyan"] = southTabCyanTexture;
        Texture2D southTabAmberTexture = game.Content.Load<Texture2D>("south-tab-amber");
        textures["south-tab-amber"] = southTabAmberTexture;
        Texture2D southTabOcherTexture = game.Content.Load<Texture2D>("south-tab-ocher");
        textures["south-tab-ocher"] = southTabOcherTexture;
        Texture2D southTabCapitalTexture = game.Content.Load<Texture2D>("south-tab-capital");
        textures["south-tab-capital"] = southTabCapitalTexture;

        Texture2D northTabTexture = game.Content.Load<Texture2D>("north-tab-white");
        textures["north-tab-white"] = northTabTexture;
        Texture2D northTabMagentaTexture = game.Content.Load<Texture2D>("north-tab-magenta");
        textures["north-tab-magenta"] = northTabMagentaTexture;
        Texture2D northTabCyanTexture = game.Content.Load<Texture2D>("north-tab-cyan");
        textures["north-tab-cyan"] = northTabCyanTexture;
        Texture2D northTabAmberTexture = game.Content.Load<Texture2D>("north-tab-amber");
        textures["north-tab-amber"] = northTabAmberTexture;
        Texture2D northTabOcherTexture = game.Content.Load<Texture2D>("north-tab-ocher");
        textures["north-tab-ocher"] = northTabOcherTexture;
        Texture2D northTabCapitalTexture = game.Content.Load<Texture2D>("north-tab-capital");
        textures["north-tab-capital"] = northTabCapitalTexture;

        textures[DIRECTION_NORTH] = northTabTexture;
        textures[DIRECTION_SOUTH] = southTabTexture;

        Texture2D magentaTank = game.Content.Load<Texture2D>("magenta-tank-48x48");
        units["magenta-tank"] = magentaTank;
        Texture2D amberTank = game.Content.Load<Texture2D>("amber-tank-48x48");
        units["amber-tank"] = amberTank;
        Texture2D ocherTank = game.Content.Load<Texture2D>("ocher-tank-48x48");
        units["ocher-tank"] = ocherTank;
        Texture2D cyanTank = game.Content.Load<Texture2D>("cyan-tank-48x48");
        units["cyan-tank"] = cyanTank;

        Texture2D magentaInfantry = game.Content.Load<Texture2D>("magenta-infantry-48x48");
        units["magenta-infantry"] = magentaInfantry;
        Texture2D amberInfantry = game.Content.Load<Texture2D>("amber-infantry-48x48");
        units["amber-infantry"] = amberInfantry;
        Texture2D ocherInfantry = game.Content.Load<Texture2D>("ocher-infantry-48x48");
        units["ocher-infantry"] = ocherInfantry;
        Texture2D cyanInfantry = game.Content.Load<Texture2D>("cyan-infantry-48x48");
        units["cyan-infantry"] = cyanInfantry;
        Texture2D greyInfantry = game.Content.Load<Texture2D>("grey-infantry-48x48");
        units["grey-infantry"] = greyInfantry;

        // TODO: create new icon for dug-in infantry
        units["magenta-dug-in-infantry"] = magentaInfantry;
        units["amber-dug-in-infantry"] = amberInfantry;
        units["ocher-dug-in-infantry"] = ocherInfantry;
        units["cyan-dug-in-infantry"] = cyanInfantry;
        units["grey-dug-in-infantry"] = greyInfantry;

        Texture2D magentaComcen = game.Content.Load<Texture2D>("magenta-comcen-48x48");
        units["magenta-comcen"] = magentaComcen;
        Texture2D amberComcen = game.Content.Load<Texture2D>("amber-comcen-48x48");
        units["amber-comcen"] = amberComcen;
        Texture2D ocherComcen = game.Content.Load<Texture2D>("ocher-comcen-48x48");
        units["ocher-comcen"] = ocherComcen;
        Texture2D cyanComcen = game.Content.Load<Texture2D>("cyan-comcen-48x48");
        units["cyan-comcen"] = cyanComcen;

        Texture2D magentaSub = game.Content.Load<Texture2D>("magenta-sub-48x48");
        units["magenta-sub"] = magentaSub;
        Texture2D amberSub = game.Content.Load<Texture2D>("amber-sub-48x48");
        units["amber-sub"] = amberSub;
        Texture2D ocherSub = game.Content.Load<Texture2D>("ocher-sub-48x48");
        units["ocher-sub"] = ocherSub;
        Texture2D cyanSub = game.Content.Load<Texture2D>("cyan-sub-48x48");
        units["cyan-sub"] = cyanSub;

        Texture2D magentaTransportTank = game.Content.Load<Texture2D>("magenta-transport-tank-48x48");
        units["magenta-transport-tank"] = magentaTransportTank;
        Texture2D amberTransportTank = game.Content.Load<Texture2D>("amber-transport-tank-48x48");
        units["amber-transport-tank"] = amberTransportTank;
        Texture2D ocherTransportTank = game.Content.Load<Texture2D>("ocher-transport-tank-48x48");
        units["ocher-transport-tank"] = ocherTransportTank;
        Texture2D cyanTransportTank = game.Content.Load<Texture2D>("cyan-transport-tank-48x48");
        units["cyan-transport-tank"] = cyanTransportTank;

        Texture2D magentaTransportInfantry = game.Content.Load<Texture2D>("magenta-transport-infantry-48x48");
        units["magenta-transport-infantry"] = magentaTransportInfantry;
        Texture2D amberTransportInfantry = game.Content.Load<Texture2D>("amber-transport-infantry-48x48");
        units["amber-transport-infantry"] = amberTransportInfantry;
        Texture2D ocherTransportInfantry = game.Content.Load<Texture2D>("ocher-transport-infantry-48x48");
        units["ocher-transport-infantry"] = ocherTransportInfantry;
        Texture2D cyanTransportInfantry = game.Content.Load<Texture2D>("cyan-transport-infantry-48x48");
        units["cyan-transport-infantry"] = cyanTransportInfantry;
        Texture2D greyTransportInfantry = game.Content.Load<Texture2D>("grey-transport-infantry-48x48");
        units["grey-transport-infantry"] = greyTransportInfantry;

        Texture2D magentaBattleship = game.Content.Load<Texture2D>("magenta-battleship-48x48");
        units["magenta-battleship"] = magentaBattleship;
        Texture2D amberBattleship = game.Content.Load<Texture2D>("amber-battleship-48x48");
        units["amber-battleship"] = amberBattleship;
        Texture2D ocherBattleship = game.Content.Load<Texture2D>("ocher-battleship-48x48");
        units["ocher-battleship"] = ocherBattleship;
        Texture2D cyanBattleship = game.Content.Load<Texture2D>("cyan-battleship-48x48");
        units["cyan-battleship"] = cyanBattleship;

        Texture2D magentaCarrier = game.Content.Load<Texture2D>("magenta-carrier-48x48");
        units["magenta-carrier"] = magentaCarrier;
        Texture2D amberCarrier = game.Content.Load<Texture2D>("amber-carrier-48x48");
        units["amber-carrier"] = amberCarrier;
        Texture2D ocherCarrier = game.Content.Load<Texture2D>("ocher-carrier-48x48");
        units["ocher-carrier"] = ocherCarrier;
        Texture2D cyanCarrier = game.Content.Load<Texture2D>("cyan-carrier-48x48");
        units["cyan-carrier"] = cyanCarrier;

        Texture2D magentaSpy = game.Content.Load<Texture2D>("magenta-spy-48x48");
        units["magenta-spy"] = magentaSpy;
        Texture2D amberSpy = game.Content.Load<Texture2D>("amber-spy-48x48");
        units["amber-spy"] = amberSpy;
        Texture2D ocherSpy = game.Content.Load<Texture2D>("ocher-spy-48x48");
        units["ocher-spy"] = ocherSpy;
        Texture2D cyanSpy = game.Content.Load<Texture2D>("cyan-spy-48x48");
        units["cyan-spy"] = cyanSpy;

        units["amber-decoy-comcen"] = game.Content.Load<Texture2D>("amber-decoy-comcen-48x48");
        units["cyan-decoy-comcen"] = game.Content.Load<Texture2D>("cyan-decoy-comcen-48x48");
        units["magenta-decoy-comcen"] = game.Content.Load<Texture2D>("magenta-decoy-comcen-48x48");
        units["ocher-decoy-comcen"] = game.Content.Load<Texture2D>("ocher-decoy-comcen-48x48");

        // magenta-plane-white-30px
        // magenta-plane-black-30px
        // magenta-plane-transparent-30px
        // magenta-plane-whitef-30px
        // magenta-plane-blackf-30px
        Texture2D magentaPlane = game.Content.Load<Texture2D>("magenta-plane-black-30px");
        units["magenta-plane"] = magentaPlane;
        Texture2D amberPlane = game.Content.Load<Texture2D>("amber-plane-black-30px");
        units["amber-plane"] = amberPlane;
        Texture2D cyanPlane = game.Content.Load<Texture2D>("cyan-plane-black-30px");
        units["cyan-plane"] = cyanPlane;
        Texture2D ocherPlane = game.Content.Load<Texture2D>("ocher-plane-black-30px");
        units["ocher-plane"] = ocherPlane;

        textures["ocher-order"] = game.Content.Load<Texture2D>("gc-ocher-order");
        textures["amber-array"] = game.Content.Load<Texture2D>("gc-amber-array");
        textures["cyan-circle"] = game.Content.Load<Texture2D>("gc-cyan-circle");
        textures["magenta-mob"] = game.Content.Load<Texture2D>("gc-magenta-mob");

        Texture2D hexHighlight = game.Content.Load<Texture2D>("YellowHexagonOutline_72x72");
        textures["mapHexHighlight"] = hexHighlight;

        Texture2D warTexture = game.Content.Load<Texture2D>("swords");
        textures["war"] = warTexture;

        Texture2D teamMatesTexture = game.Content.Load<Texture2D>("marriage");
        textures["team-mates"] = teamMatesTexture;

        Texture2D allianceTexture = game.Content.Load<Texture2D>("alliance");
        textures["alliance"] = allianceTexture;

        Texture2D ceaseFireTexture = game.Content.Load<Texture2D>("cease-fire");
        textures["cease-fire"] = ceaseFireTexture;
        textures[RESOURCE_MINERAL_DEPOSITS] =  game.Content.Load<Texture2D>("gold-mining-cart");
        textures[RESOURCE_FUEL] =  game.Content.Load<Texture2D>("oil");

        foreach (string key in burbs.Keys)
        {
            textures[key] = burbs[key];
        }

        foreach (string key in units.Keys)
        {
            textures[key] = units[key];
        }
    }
}
