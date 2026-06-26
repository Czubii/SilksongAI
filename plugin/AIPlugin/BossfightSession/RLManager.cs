using AIPlugin.Networking;
using AIPlugin.Networking.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AIPlugin.BossfightSession
{
    public class RLManager: MonoBehaviour
    {
        private enum State
        {
            StartingNew,
            AwaitingResponse,
            StartingFight,
            Fighting
        }
        private State _state;
        private bool epochActive;
        private Action _startAction;

        private SessionDispatcher _dispatcher;
        private AiService _service;
        private Requests.RLCanRunFight _canRunFightRequest;

        public void Initialize(SessionDispatcher dispatcher, AiService service)
        {
            _dispatcher = dispatcher;
            _service = service;

            _service.Gateway.Events.OnRLEpoch += OnRLEpch;
        }
        private void OnRLEpch()
        {
            if (!enabled) return;
            epochActive = true;
            _state = State.StartingNew;
        }
        private void Update()
        {
            if (!epochActive) return;

            switch (_state)
            {
                case State.StartingNew:
                    if (_dispatcher.CanStartSession())
                    {
                        _canRunFightRequest = new Requests.RLCanRunFight(_service.Gateway);
                        _canRunFightRequest.OnSuccess += OnRunResponse;
                        _state = State.AwaitingResponse;
                    }
                    break;

                case State.AwaitingResponse:
                    break;

                case State.StartingFight:
                    if(!_dispatcher.IsSessionActive() && _dispatcher.CanStartSession())
                    {
                        _startAction?.Invoke();
                        _state = State.Fighting;
                    }
                    break;

                case State.Fighting:
                    if (!_dispatcher.IsSessionActive())
                    {
                        _state = State.StartingNew;
                    }
                    break;

            }


        }
        private void OnRunResponse(Requests.RLCanRunFight.Response response)
        {
            if (!response.CanRun)
            {
                epochActive = false;
                _state = State.StartingNew;
                return;
            }
            _startAction = () => { _dispatcher.StartRLFight(response.TargetBossName); };
            _state = State.StartingFight;
        }
    }
}
