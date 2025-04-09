
/// <summary>
/// ��Ϸ���ж�̬����
/// </summary>
public class MC_Runtime_DynamicData
{
    private static MC_Runtime_DynamicData _instance;
    public static MC_Runtime_DynamicData instance => _instance ??= new MC_Runtime_DynamicData();
    private MC_Runtime_DynamicData() { }// ˽�й��캯�����������ⲿֱ�� new

    //GameState
    private Game_State _game_state = Game_State.Start;
    private GameMode _game_mode = GameMode.Survival;

    
    public void SetGameState(Game_State newState)
    {
        _game_state = newState;
    }

    public void SetGameMode(GameMode newMode)
    {
        _game_mode = newMode;
    }

    public Game_State GetGameState()
    {
        return _game_state;
    }

    public GameMode GetGameMode()
    {
        return _game_mode;
    }
}
