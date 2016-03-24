using System;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour {

    public float mainSpawnDelay = 0.4f;		// Spawn each enemy after certain time

	public List<SpawnPosition> spawnPositions = new List<SpawnPosition>();
	public EnemyType[] enemyTypes;
    public Wave[] waves;

    private Enemy[] currentEnemies; 	    // All our currently spawned enemies
	private int startingAmount = 8;			// Start amount of emenies
	private int addEachWave = 5;			// How much we will add each wave
                                               
	private int amountToSpawn = 0;			// Current amount of enemies
	private int spawnedAmount = 0;			// How much we have spawned
	private int currentlyAlive = 0;            
	private int leftAmount = 0;                
	private int MAX_ENEMY_AMOUNT = 8;       // Maximum amount of emenies in scene
                                               
	private int currentWave = 1;		    // Current wave
	private const byte waitBeforeSpawn = 5; // How much wait before each wave spawning

    static SpawnManager _instance;

    public static SpawnManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindGameObjectWithTag("Game").GetComponent<SpawnManager>();
                if (_instance == null)
                {
                    return null;
                }
            }
            return _instance;
        }
    }
	
	[Serializable]
	public class SpawnPosition
    {
		public Transform transform;	//drag and drop from inspector
		public bool canBeUsed;		//can this spawn be used, in case you to lock certain points
		public float spawnDelay;    //delay for each spawned enemy
        public int teamID;

        public SpawnPosition(Vector2Int position, int teamID)
        {
            this.transform = new GameObject("SpawnPoint", typeof(Spawn)).transform;
            this.transform.position = position.ToTileVector2();
            this.teamID = teamID;
            canBeUsed = true;
        }
	};	
   
    /// <summary>
    /// This is for keeping needed enemies to be spawned
    /// </summary>
    private struct Enemy{
		public int index; 		//Index of the enemy type
		public bool spawned;	//when this enemy is spawned we toggle it as true, so we would not spawn this specific enemy again
	};
    
	[Serializable]
	public class EnemyType{
		public GameObject enemyPrefab;		//Drag your enemy prefab here
		public int[] preferedSpawnPoints;	//In case special type of enemy can (or should) spawn at specific locations, if size = 0 -> no prefered locations
											//Numbers should be indexes of <spawnPositions> array you created before
		public bool onStart;				//Should this enemy be spawned on beggining of actual waves, if not you can set him/them manually using wave class from inspector	
		public float ratio;					//This is used only if <onStart> is false, for example, if ratio is 1.5 and amount of enemies this wave is 15, then this will be spawned 1 time.|.
	};

    /// <summary>
    /// In case you want to unlock specific enemies only after certain wave
    /// </summary>
    [Serializable]
    public class Wave_Enemy{
		public int index;
		public int amount;
	}

	[Serializable]
	public class Wave
    {
		public int wave; 					//wave we want to affect with these changes
		public Wave_Enemy[] enemies;		//enemies we want to add/spawn only this wave
		public bool spawnOnlyThisWave;		//Add for all continuing waves OR spwn just for this wave
	};

    [Serializable]
    public class Teams
    {
        public Spawn[] team1Spawns;
        public Spawn[] team2Spawns;      
    };
    
	/// <summary>
    /// This will change spawn delay for certain spawn point
    /// </summary>
    private void SpawnDelay(short index, float newDelay)
    {
        if (index >= spawnPositions.Count || index < 0)
            Debug.Log("ERROR:[SpawnDelay] Received index " + index + " is not a valid index.");
        else
            spawnPositions[index].spawnDelay = newDelay;    //Set new delay for spawn position
	}

    /// <summary>
    /// This will extract the spawnpoints from the map data's entities
    /// </summary>
    public void SetupSpawnPoints()
    {
        spawnPositions.Clear();

        var entities = FindObjectOfType<UnityTileMap.TileMapBehaviour>().m_entities;
        foreach(var entity in entities)
        {
            if(entity.Value == EntitiesData.EntityID.Info_CT || entity.Value == EntitiesData.EntityID.Info_T)
            {
                spawnPositions.Add(new SpawnPosition(entity.Key, (int)entity.Value));
            }
        }
    }

    public Vector3 GetRandomSpawnPosition()
    {
        SetupSpawnPoints();

        int[] spawnPointIndexes = new int[spawnPositions.Count];
        for (int i = 0; i < spawnPositions.Count; i++)
        {
            spawnPointIndexes[i] = i;
        }
        int randomPos = UnityEngine.Random.Range(0, spawnPointIndexes.Length);

        for (int j = 0; j < spawnPointIndexes.Length; j++)
        {
            int spawnArrIndex = spawnPointIndexes[randomPos];
            //can this spawn be accessed
            if (spawnPositions[spawnArrIndex].canBeUsed)
            {
                //is there a cool down on this spawn point
                if (spawnPositions[spawnArrIndex].transform.gameObject.GetComponent<Spawn>().Status())
                {
                    return spawnPositions[spawnArrIndex].transform.position;
                }
            }
        }
        return Vector3.one;
    }

    public Vector3 GetTeamSpawnPosition(int teamID)
    {
        SetupSpawnPoints();

        int[] spawnPointIndexes = new int[spawnPositions.Count];
        for (int i = 0; i < spawnPositions.Count; i++)
        {
            spawnPointIndexes[i] = i;
        }
        int randomPos = UnityEngine.Random.Range(0, spawnPointIndexes.Length);

        for (int j = 0; j < spawnPointIndexes.Length; j++)
        {
            int spawnArrIndex = spawnPointIndexes[randomPos];
            //can this spawn be accessed
            if (spawnPositions[spawnArrIndex].canBeUsed && spawnPositions[spawnArrIndex].teamID == teamID)
            {
                //is there a cool down on this spawn point
                if (spawnPositions[spawnArrIndex].transform.gameObject.GetComponent<Spawn>().Status())
                {
                    return spawnPositions[spawnArrIndex].transform.position;
                }
            }
        }
        return Vector3.one;
    }

    /// <summary>
    /// Spawns actual emenies
    /// </summary>
    private void WaveSpawner(){
		//all enemies have not been spawned, yet
		if(! GetAllEnemiesStatus()){
			//check if scene already does not contain max enemies
			if(currentlyAlive < MAX_ENEMY_AMOUNT){

				for(int i =0; i <currentEnemies.Length; i++){
					//This enemy has not been spawned yet
					if(!currentEnemies[i].spawned){
						int thisEnemyTypeIndex = currentEnemies[i].index;

						//============Does this enemy type have preffered spawn point?=======
						//Spawn point indexes which will be used for this enemy
						int[] spawnPointIndexes;
						int prefferedSpawnPointsIndexesSize = enemyTypes[ thisEnemyTypeIndex ].preferedSpawnPoints.Length;
						//no preffered spawn points for this enemy
						if(prefferedSpawnPointsIndexesSize == 0){
							spawnPointIndexes = new int[spawnPositions.Count];
							//assign positions
							for(int yyy =0; yyy <spawnPositions.Count; yyy++){
								spawnPointIndexes[yyy] = yyy;
							}
						}
						//enemy does have preffered spawn points
						else{
							spawnPointIndexes = new int[ prefferedSpawnPointsIndexesSize ];
							
							//assign positions for preffered spawn points
							for(int yy =0; yy < prefferedSpawnPointsIndexesSize; yy++){
								int prefIndex = enemyTypes[ thisEnemyTypeIndex ].preferedSpawnPoints[yy]; //get the index
								
								spawnPointIndexes[yy] = prefIndex;
							}
						}
						//===================================================================
						//random spawn destination
						int randomPos = UnityEngine.Random.Range(0, spawnPointIndexes.Length);

						for(int j =0; j <spawnPointIndexes.Length; j++){
							int spawnArrIndex = spawnPointIndexes[randomPos];
							//can this spawn be accessed
							if(spawnPositions[spawnArrIndex].canBeUsed){
								//is there a cool down on this spawn point
								if(spawnPositions[spawnArrIndex].transform.gameObject.GetComponent<Spawn>().Status()){
									Instantiate(enemyTypes[ thisEnemyTypeIndex ].enemyPrefab, spawnPositions[spawnArrIndex].transform.position, Quaternion.identity);

									//mark this enemy as spawned
									currentEnemies[i].spawned = true;
									currentlyAlive++;
									spawnedAmount++;
									
									//delay for spawn point
									spawnPositions[spawnArrIndex].transform.gameObject.GetComponent<Spawn>().Spawned( spawnPositions[spawnArrIndex].spawnDelay );

									return;
								}
							}

							//increment random spawn position
							randomPos++;
							if(randomPos >= spawnPointIndexes.Length-1) randomPos = 0; //be sure it does contain corretn index
						}
					}
				}
			}
		}

		else if(currentlyAlive <= 0){
			CancelInvoke();
			WaveFinished(false);
		}
	}

    /// <summary>
    /// This is called when wave is finished
    /// </summary>
    private void WaveFinished(bool firstLaunch)
    {
		if(!firstLaunch) currentWave ++; //add wave count

		else
        {
			if(MAX_ENEMY_AMOUNT < 20){
				MAX_ENEMY_AMOUNT++;
			}
		}

		spawnedAmount = 0;
		currentlyAlive = 0;
		amountToSpawn = (startingAmount + (addEachWave* (currentWave-1))); //add amount of enemies for next wave



		leftAmount = EnemyAmountLeft(); //calculate how much enemies there will be next wave
		SetSpawnEnemiesIndexes ();		//Set current Enemies structure, what enemies to spawn and their order
    }

	private void SpawnNextWave()
    {
		InvokeRepeating("Spawner",0, mainSpawnDelay); 
	}

    /// <summary>
    /// Returns amount, how much emenies we still have to spawn
    /// </summary>
    private int EnemyAmountLeft()
    {
		return amountToSpawn - spawnedAmount;
	}

    /// <summary>
    /// Returns true if all enemies are spawned in this wave, else, returns false
    /// </summary>
    private bool GetAllEnemiesStatus()
    {
		for(int i = 0; i < currentEnemies.Length; i++)
        {
			if(! currentEnemies[i].spawned) return false;
		}

		return true;
	}

    /// <summary>
    /// Create enemy type index array from enemy types indes - array of enemy numbers to be spawned
    /// </summary>
    private void SetSpawnEnemiesIndexes()
    {
		bool specialWaveEvent = false;
		int specialWave = 0;
		//check if there is special "WAVE" event
		for(int iwave = 0; iwave < waves.Length; iwave++){
			if(waves[iwave].wave == currentWave){
				specialWaveEvent = true;
				specialWave = iwave;
				break;
			}
		}


		//SPECIAL WAVE EVENT
		if(specialWaveEvent){
			int amountOfEnemies = 0;

			//find out how much enemies there will be this round (from wave event)
			for(int qq = 0; qq < waves[specialWave].enemies.Length; qq++){
				amountOfEnemies += waves[specialWave].enemies[qq].amount;
			}

			//create array of next wave enemies
			currentEnemies = new Enemy[amountOfEnemies];
			//set everything to default
			ResetCurrentEnemies();

			//enemies lenght
			for(int tt =0; tt < waves[specialWave].enemies.Length; tt++){
				//amount of each enemy
				for(int uu1 =0; uu1 <waves[specialWave].enemies[tt].amount; uu1++){
					int thisEnemyIndex = waves[specialWave].enemies[tt].index;

					int randomPos = UnityEngine.Random.Range(0, currentEnemies.Length);
					//maybe the saved current enemy is busy already, increment till we find empty spot
					while(currentEnemies[randomPos].index != -1){

						randomPos++;
						if(randomPos > currentEnemies.Length-1) randomPos = 0;
					}

					//save enemy type index in array
					currentEnemies[randomPos].index = thisEnemyIndex;
				}
			}

			amountToSpawn = amountOfEnemies;
		}

		//ORIDINARY WAVE
		else{
			//create array of next wave enemies
			currentEnemies = new Enemy[leftAmount];
			//set everything to default
			ResetCurrentEnemies();

			//Loops through all enemy types, to get the ratio and calculate how much this wave they should be spawned
			//We start from last enemy type because enemy with index 0 is the default enemy which all empty (-1) spots will be filled
			for(int nn = enemyTypes.Length-1; nn >= 1; nn--){
				//can this monster be used on start?
				if(enemyTypes[nn].onStart){
					float ratioAmount = leftAmount/10f;

					//ratioAmount /= enemyTypes[nn].ratio;
					//int spawnAmount = Mathf.FloorToInt(ratioAmount);
					int spawnAmount = Mathf.FloorToInt( ratioAmount*enemyTypes[nn].ratio );

					if(spawnAmount > 0){
						//Sets actual enemy indexes in array
						for(int tt =0; tt <spawnAmount; tt++){
							int randomPos = UnityEngine.Random.Range(0, currentEnemies.Length);
							//maybe the saved current enemy is busy already, increment till we find empty spot
							while(currentEnemies[randomPos].index != -1){
								randomPos++;
								if(randomPos > currentEnemies.Length-1) randomPos = 0;
							}

							//save enemy type index in array
							currentEnemies[randomPos].index = nn;
						}
					}
				}
			}

			//All "special enemy" slots are filled, now fill up the empty slots with default enemies
			for(int w = 0; w < currentEnemies.Length; w++){
				if(currentEnemies[w].index == -1){
					currentEnemies[w].index = 0;
				}
			}
		}
	}

    private void Start()
    {
        SetupSpawnPoints();
    }

    /// <summary>
    /// Reset current enemies to default
    /// </summary>
    private void ResetCurrentEnemies()
    {
		for(int vv = 0; vv < currentEnemies.Length; vv++){
			currentEnemies[vv].index = -1;
			currentEnemies[vv].spawned = false;
		}
	}
}
