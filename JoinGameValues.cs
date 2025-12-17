namespace GlobalConquest;

public class JoinGameValues
{
    public int Port { get; set;  }

    public string HostIp { get; set;  }

    public string Name { get; set; }

    public string FactionName { get; set; }
    public bool IsObserverOnly {get; set;}
    public int GameExecutionSpeed {get; set;} = 10;
    Dictionary<string, int> animalToSpeed = new Dictionary<string, int>();
    List<string> animals = ["snail", "turtle", "rabbit", "jaguar", "falcon"];
    int currentAnimalIndex = 2;
    public int SoundVolume {get; set;} = 50;

    public JoinGameValues()
    {
        animalToSpeed["snail"] = 100;
        animalToSpeed["turtle"] = 50;
        animalToSpeed["rabbit"] = 10;
        animalToSpeed["jaguar"] = 5;
        animalToSpeed["falcon"] = 1;
    }

    public void setGameExecutionSpeed(string animal)
    {
        if (animalToSpeed.ContainsKey(animal))
        {
            GameExecutionSpeed = animalToSpeed[animal];
            int index = 0;
            foreach (string searchAnimal in animals)
            {
                if (animal.Equals(searchAnimal))
                {
                    currentAnimalIndex = index;
                    break;
                }
                index += 1;
            }
        }
        else
        {
            Globals.Log("setGameExecutionSpeed(): " + animal + " not valid. Defaulting to 100ms.");
            GameExecutionSpeed = animalToSpeed["rabbit"];
            currentAnimalIndex = 2;
        }
    }

    public string increaseGameExecutionSpeed()
    {
        string animal = animals[currentAnimalIndex];
        Globals.Log("increaseGameExecutionSpeed(): current " + animal + " " + GameExecutionSpeed);
        if (currentAnimalIndex < 4)
            currentAnimalIndex += 1;
        animal = animals[currentAnimalIndex];
        GameExecutionSpeed = animalToSpeed[animal];
        Globals.Log("increaseGameExecutionSpeed(): return " + animal + " " + GameExecutionSpeed);
        return animal;
    }

    public string decreaseGameExecutionSpeed()
    {
        string animal = animals[currentAnimalIndex];
        Globals.Log("decreaseGameExecutionSpeed(): current " + animal + " " + GameExecutionSpeed);
        if (currentAnimalIndex > 0)
            currentAnimalIndex -= 1;
        animal = animals[currentAnimalIndex];
        GameExecutionSpeed = animalToSpeed[animal];
        Globals.Log("decreaseGameExecutionSpeed(): return " + animal + " " + GameExecutionSpeed);
        return animal;
    }

    public string getGameSpeed()
    {
        return animals[currentAnimalIndex];
    }
    public int getGameSpeedIndex()
    {
        return currentAnimalIndex;
    }

    public string getNextFasterGameSpeed()
    {
        //Globals.Log("getNextFasterGameSpeed(): enter");
        int index = currentAnimalIndex;
        if (index < 4)
            index += 1;
        //Globals.Log("getNextFasterGameSpeed(): " + animals[index]);
        return animals[index];
    }
    public string getNextSlowerGameSpeed()
    {
        //Globals.Log("getNextSlowerGameSpeed(): enter");
        int index = currentAnimalIndex;
        if (index > 0)
            index -= 1;
        //Globals.Log("getNextSlowerGameSpeed(): " + animals[index]);
        return animals[index];
    }

    public int getGameSpeedMs()
    {
        return GameExecutionSpeed;
    }

}