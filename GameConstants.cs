using System.Data;

public static class GameConstants
{
    public const string TERRAIN_UNKNOWN = "unknown";
    public const string TERRAIN_GRASS = "grass";
    public const string TERRAIN_SEA = "sea";
    public const string TERRAIN_MOUNTAIN = "mountain";
    public const string TERRAIN_SWAMP = "swamp"; // marsh
    public const string TERRAIN_FOREST = "forest";
    // desert
    public const string TERRAIN_LAND = "land";

    public const string NATIVE_COLOR = "grey";
    public const string AMBER = "amber";
    public const string OCHER = "ocher";
    public const string CYAN = "cyan";
    public const string MAGENTA = "magenta";
    public static readonly List<string> FACTION_COLORS = [AMBER, CYAN, MAGENTA, OCHER];
    public static readonly List<string> NATIVE_AND_FACTION_COLORS = [NATIVE_COLOR, AMBER, CYAN, MAGENTA, OCHER];

    public const string AI_GOAL_CONQUER = "conquer";
    public const string AI_GOAL_DEFEND = "defend";
    public const string AI_GOAL_EXPLORE = "explore";
    public const string AI_GOAL_BUILD_PLANE = "build-plane";
    public const string AI_GOAL_BUILD_CARRIER = "build-carrier";

    public const string TREATY_AT_WAR = "war";
    public const string TREATY_CEASE_FIRE = "cease fire";
    public const string TREATY_ALLIANCE = "alliance";
    public const string TREATY_TEAM_MATES = "team-mates";

    public const string VISIBILITY_OMNISCIENT = "Omniscient";
    public const string VISIBILITY_COMMAND_HQ = "Command HQ";
    public const string VISIBILITY_FOG = "Fog of War";

    public const string EXECUTION_QUORUM = "Quorum";
    public const string EXECUTION_TIMED = "Timed*";
    public const string EXECUTION_GRACE = "Grace*";
    public const string EXECUTION_IMMEDIATE = "Immediate";

    public const string EVENT_TYPE_MAP_UPDATE = "mapUpdate";
    public const string EVENT_TYPE_GAME_STATE_UPDATE = "gameStateUpdate";
    public const string EVENT_TYPE_GAME_STATE_AND_MAP_UPDATE = "gameStateAndMapUpdate";
    public const string FACTION_STATUS_DISCONNECTED = "disconnected";
    public const string FACTION_STATUS_PLANNING = "planning";
    public const string GAME_PHASE_PLAN = "plan";
    public const string TAG_MINI_MAP = "miniMap";
    public const string MAP_ORIENTATION_HORIZONTAL = "horizontal";
    public const string MAP_ORIENTATION_VERTICAL = "vertical";
    public const string MAP_ORIENTATION_BALANCED = "balanced";
    public const string VICTORY_HEAD_COUNT = "Head-Count";
    public const string VICTORY_INCOME = "Income";
    public const string VICTORY_COMBINED = "Combined";

    // higher number at bottom
    public const float LAYER_TERRAIN = 0.85f;
    public const float LAYER_HEX_HIGHLIGHT = 0.55f;
    public const float LAYER_RESOURCE = 0.75f;
    public const float LAYER_BURB = 0.60f;
    public const float LAYER_UNIT = 0.50f;
    public const float LAYER_PLANE = 0.35f;
}




