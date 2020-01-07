using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace PrototypeConsole
{
    class Program
    {
        //static bool isFiveAct;    //TODO?


        static List<Scene> possibleScenes;

        static bool planIsValid = false;

        static NGOAP.NGOAP planner;


        static void Main(string[] args)
        {
            //initialisation
            Console.Title = "Narrative GOAP Console Prototype";
            Console.WriteLine("Initialising...");
            if (args.Length == 0)
            {
                Console.WriteLine("No XML file given!\nProgram will terminate - press any key");
                Console.ReadKey();
                return;
            }
            initNGOAP(args[0]);
            Console.WriteLine("Initialised.");

            Console.WriteLine("When prompted for input, entering \"exit\" or \"halt\" will terminate the program.\nPress any key to continue.");
            Console.ReadKey();

            //main interaction loop
            while (true)
            {
                if (!planIsValid)   //if there is currently no valid plan
                {
                    //create new plan
                    Console.WriteLine("Planning path...");
                    
                    if (planner.makePlan() != 0)   //get a new plan
                    {
                        Console.WriteLine("Planning was NOT successfull!");
                        Console.ReadKey();
                        break;                      //terminate
                    }

                    planIsValid = true;
                    Console.WriteLine("Planning successfull!");

                    Console.WriteLine(planner.planToString());
                }

                //Console.Clear();
                if (planner.checkGoalReached())  //check if we are at the current goal
                {
                    //reached the end of the plan - goal is fulfilled
                    Console.WriteLine("\nYou have reached the goal:\n" + planner.getCurrentGoal().id + "\n\n FIN\n\nYou have reached the end - press any key to quit");
                    Console.ReadKey();
                    break;
                }

                Scene currentScene;
                currentScene = getSceneFromActionID(planner.getNextAction());

                //Output
                Console.WriteLine("\n\n" + currentScene.id + "\n\n" + currentScene.output + "\n");
                foreach (string item in currentScene.outcomes.Keys)
                    Console.WriteLine("Option: " + item);

                //Input
                Console.Write("\n>");
                string input = Console.ReadLine();
                if (input == "exit" || input == "halt")     //terminate program when user enters "exit" or "halt"
                    break;

                //else check if the input is valid
                if (!currentScene.outcomes.ContainsKey(input))
                {
                    Console.WriteLine("Invalid Input");
                    continue;
                }
                else
                {   //input string is a valid reaction
                    Console.WriteLine("Valid Input\n");

                    string actionID = planner.getNextAction();
                    //apply the chosen outcome to current worldstate
                    planner.applyAction(getSceneIDFromActionID(actionID) + "_" + input);

                    //check wether the reaction differs from the plan
                    if (getSceneOutcomeFromActionID(actionID) != input)
                    {
                        //deviation from plan
                        Console.WriteLine("Plan is no longer valid!");
                        planIsValid = false;
                        planner.invalidatePlan();
                    }
                }

            }

            //Clean up
            //...
            Console.Clear();
            Console.WriteLine("Program successfully terminated...\nPress any key to close.");
            Console.ReadKey();
        }

        static void initNGOAP(string XMLfile)
        {
            //initialise NGOAP planner
            planner = new NGOAP.NGOAP();

            possibleScenes = new List<Scene>();

            readXML(XMLfile);
            Console.WriteLine("XML read!");

            foreach (Scene scene in possibleScenes)
                createActionFromScene(scene);
        }

        static void createActionFromScene(Scene scene)
        {
            foreach (KeyValuePair<string, Dictionary<string, bool>> item in scene.outcomes)
            {
                string id = scene.id + "_" + item.Key;              //Action.id : "SCENEID_OUTCOMEID"
                planner.createAction(id, scene.conditions, item.Value);
            }
        }

        static Scene getSceneFromActionID(string actionID)
        {
            string id = actionID.Remove(actionID.IndexOf('_'));     //gets the string before the underscore
            foreach (Scene item in possibleScenes)
                if (item.id == id)
                    return item;
            return null;
        }

        static string getSceneIDFromActionID(string actionID)
        {
            return actionID.Remove(actionID.IndexOf('_'));          //gets the string before the underscore
        }

        static string getSceneOutcomeFromActionID(string actionID)
        {
            return actionID.Substring(actionID.IndexOf('_') + 1);   //gets the string after the underscore
        }

        static void readXML(string filename)
        {
            XmlReader reader = XmlReader.Create(filename);

            while (reader.Read())
            {
                if (reader.IsStartElement())
                {
                    switch (reader.Name)
                    {
                        case "Goal":
                            string goalId = reader.GetAttribute("id");
                            reader.MoveToAttribute("priority");
                            int goalPriority = reader.ReadContentAsInt();
                            Dictionary<string, bool> goalConditions = new Dictionary<string, bool>();
                            while (reader.Read())
                            {
                                if(reader.IsEmptyElement)
                                {   //read condition tags
                                    reader.MoveToFirstAttribute();
                                    string conditionID = reader.Name;
                                    bool conditionValue = reader.ReadContentAsBoolean();
                                    goalConditions.Add(conditionID, conditionValue);
                                }
                                if (reader.NodeType == XmlNodeType.EndElement)
                                    break;
                            }
                            planner.createGoal(goalId, goalConditions, goalPriority);
                            break;
                        case "Scene":
                            string sceneId = reader.GetAttribute("id");
                            string sceneOutput = "<NO OUTPUT>";
                            Dictionary<string, bool> preconditions = new Dictionary<string, bool>();
                            Dictionary<string, Dictionary<string, bool>> outcomes = new Dictionary<string, Dictionary<string, bool>>();

                            while (reader.Read())
                            {
                                if (reader.IsStartElement("Output"))
                                {
                                    sceneOutput = reader.ReadElementContentAsString();
                                    continue;
                                }
                                if (reader.IsEmptyElement)
                                {   //read condition tags
                                    reader.MoveToFirstAttribute();
                                    string conditionID = reader.Name;
                                    bool conditionValue = reader.ReadContentAsBoolean();
                                    preconditions.Add(conditionID, conditionValue);
                                }
                                if (reader.IsStartElement("Outcome"))
                                {   //read outcome tags
                                    string outComeId = reader.GetAttribute("id");
                                    Dictionary<string, bool> effects = new Dictionary<string, bool>();

                                    while (reader.Read())
                                    {
                                        if (reader.IsEmptyElement)
                                        {   //read effect tags
                                            reader.MoveToFirstAttribute();
                                            string effectID = reader.Name;
                                            bool effectValue = reader.ReadContentAsBoolean();
                                            effects.Add(effectID, effectValue);
                                        }
                                        if (reader.NodeType == XmlNodeType.EndElement)
                                            break;
                                    }
                                    reader.Read();
                                    effects.Add("SceneIsDone" + sceneId, true); //make sure outcomes disable the whole scene
                                    outcomes.Add(outComeId, effects);
                                }
                                if (reader.NodeType == XmlNodeType.EndElement)
                                    break;
                            }
                            preconditions.Add("SceneIsDone" + sceneId, false);
                            possibleScenes.Add(new Scene(sceneId,sceneOutput,preconditions,outcomes));
                            break;
                        case "StartState":
                            Dictionary<string, bool> startState = new Dictionary<string, bool>();
                            while (reader.Read())
                            {
                                if (reader.IsEmptyElement)
                                {   //read condition tags
                                    reader.MoveToFirstAttribute();
                                    string conditionID = reader.Name;
                                    bool conditionValue = reader.ReadContentAsBoolean();
                                    startState.Add(conditionID, conditionValue);
                                }
                                if (reader.NodeType == XmlNodeType.EndElement)
                                    break;
                            }
                            planner.createStartState(startState);
                            break;
                    }
                }
            }
            reader.Close();
        }
    }
}