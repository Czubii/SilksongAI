using AIPlugin.PluginGUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIPlugin.BossfightSession.Agents
{
    public class AgentManager
    {
        public enum AgentSelection
        {
            None,
            Simple,
            ReinforcementLearning
        }
        private BaseAgent _selectedAgent = null;
        public BaseAgent SelectedAgent => _selectedAgent;

        private SimpleAgent _simpleAgent;
        private RLAgent _reinforcementLearningAgent;

        public AgentManager(SimpleAgent simpleAgent, RLAgent reinforcementLearningAgent)
        {
            _simpleAgent = simpleAgent;
            _reinforcementLearningAgent = reinforcementLearningAgent;
        }

        public void SelectAgent(AgentSelection selection)
        {
            switch (selection)
            {
                case AgentSelection.Simple:
                    _selectedAgent = _simpleAgent;
                    _simpleAgent.enabled = true;
                    _reinforcementLearningAgent.enabled = false;
                    break;
                case AgentSelection.ReinforcementLearning:
                    _selectedAgent = _reinforcementLearningAgent;
                    _simpleAgent.enabled = false;
                    _reinforcementLearningAgent.enabled = true;
                    break;
                default:
                    _selectedAgent = null;
                    _simpleAgent.enabled = false;
                    _reinforcementLearningAgent.enabled = false;
                    break;
            }
        }
    }
}
