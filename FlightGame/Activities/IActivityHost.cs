namespace FlightGame.Activities;

public interface IActivityHost
{
    void SetActivity(IActivity activity);

    void ExitGame();
}
