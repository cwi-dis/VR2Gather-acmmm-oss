﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OrchestratorWSManagement;
using LitJson;

namespace OrchestratorWrapping
{
    // class that describes the status for the response from the orchestrator
    public class ResponseStatus
    {
        public int Error;
        public string Message;

        public ResponseStatus(int error, string message)
        {
            this.Error = error;
            this.Message = message;
        }
        public ResponseStatus() : this(0, "OK") { }
    }

    // class that encapsulates the connection with the orchestrator, emitting and receiving the events
    // and converting and parsing the camands and the responses
    public class OrchestratorWrapper : IOrchestratorConnectionListener, IMessagesListener
    {
        // manager for the socketIO connection to the orchestrator 
        private OrchestratorWSManager OrchestrationSocketIoManager;

        // Listener for the responses of the orchestrator
        private IOrchestratorResponsesListener ResponsesListener;

        // Listener for the responses of the orchestrator
        private IOrchestratorMessageListener MessagesListener;

        // List of available commands (grammar description)
        public List<OrchestratorCommand> orchestratorCommands { get; private set; }


        public OrchestratorWrapper(string orchestratorSocketUrl, IOrchestratorResponsesListener responsesListener, IOrchestratorMessageListener messagesListener)
        {
            OrchestrationSocketIoManager = new OrchestratorWSManager(orchestratorSocketUrl, this, this);
            ResponsesListener = responsesListener;
            MessagesListener = messagesListener;
            InitGrammar();
        }
        public OrchestratorWrapper(string orchestratorSocketUrl, IOrchestratorResponsesListener responsesListener) : this(orchestratorSocketUrl, responsesListener, null) { }
        public OrchestratorWrapper(string orchestratorSocketUrl) : this(orchestratorSocketUrl, null, null) { }

        #region messages listening interface implementation
        public void OnOrchestratorResponse(int status, string response)
        {
            if (MessagesListener != null) MessagesListener.OnOrchestratorResponse(status, response);
        }

        public void OnOrchestratorRequest(string request)
        {
            if (MessagesListener != null) MessagesListener.OnOrchestratorRequest(request);
        }
        #endregion

        #region commands and responses procession
        public void Connect()
        {
            if ((OrchestrationSocketIoManager != null) && (OrchestrationSocketIoManager.isSocketConnected))
            {
                OrchestrationSocketIoManager.SocketDisconnect();
            }
            OrchestrationSocketIoManager.SocketConnect();
        }

        public void OnSocketConnect()
        {
            if (ResponsesListener != null) ResponsesListener.OnConnect();
        }

        public void Disconnect()
        {
            if (OrchestrationSocketIoManager != null)
            {
                OrchestrationSocketIoManager.SocketDisconnect();
            }
        }
        public void OnSocketDisconnect()
        {
            if (ResponsesListener != null) ResponsesListener.OnDisconnect();
        }


        public bool Login(string userName, string userPassword)
        {
            OrchestratorCommand command = GetOrchestratorCommand("Login");
            command.GetParameter("userName").ParamValue = userName;
            command.GetParameter("userPassword").ParamValue = userPassword;
            return OrchestrationSocketIoManager.EmitCommand(command);
        }

        private void OnLoginResponse(OrchestratorCommand command, OrchestratorResponse response)
        {
            if (ResponsesListener != null) ResponsesListener.OnLoginResponse(new ResponseStatus(response.error, response.message), response.body["userId"].ToString());
        }

        public bool Logout()
        {
            OrchestratorCommand command = GetOrchestratorCommand("Logout");
            return OrchestrationSocketIoManager.EmitCommand(command);
        }

        private void OnLogoutResponse(OrchestratorCommand command, OrchestratorResponse response)
        {
            if (ResponsesListener != null) ResponsesListener.OnLogoutResponse(new ResponseStatus(response.error, response.message));
        }

        public bool AddSession(string scenarioId, string sessionName, string sessionDescription)
        {
            OrchestratorCommand command = GetOrchestratorCommand("AddSession");
            command.GetParameter("scenarioId").ParamValue = scenarioId;
            command.GetParameter("sessionName").ParamValue = sessionName;
            command.GetParameter("sessionDescription").ParamValue = sessionDescription;
            return OrchestrationSocketIoManager.EmitCommand(command);
        }

        private void OnAddSessionResponse(OrchestratorCommand command, OrchestratorResponse response)
        {
            ResponseStatus status = new ResponseStatus(response.error, response.message);
            Session session = Session.ParseJsonData<Session>(response.body);
            if (ResponsesListener != null) ResponsesListener.OnAddSessionResponse(status, session);
        }


        public bool GetSessions()
        {
            OrchestratorCommand command = GetOrchestratorCommand("GetSessions");
            return OrchestrationSocketIoManager.EmitCommand(command);
        }

        private void OnGetSessionsResponse(OrchestratorCommand command, OrchestratorResponse response)
        {
            ResponseStatus status = new ResponseStatus(response.error, response.message);
            List<Session> list = Helper.ParseElementsList<Session>(response.body);
            if (ResponsesListener != null) ResponsesListener.OnGetSessionsResponse(status, list);
        }

        public bool GetSessionInfo()
        {
            OrchestratorCommand command = GetOrchestratorCommand("GetSessionInfo");
            return OrchestrationSocketIoManager.EmitCommand(command);
        }

        private void OnGetSessionInfoResponse(OrchestratorCommand command, OrchestratorResponse response)
        {
            ResponseStatus status = new ResponseStatus(response.error, response.message);
            Session session = Session.ParseJsonData<Session>(response.body);
            if (ResponsesListener != null) ResponsesListener.OnGetSessionInfoResponse(status, session);
        }

        public bool DeleteSession(string sessionId)
        {
            OrchestratorCommand command = GetOrchestratorCommand("DeleteSession");
            command.GetParameter("sessionId").ParamValue = sessionId;
            return OrchestrationSocketIoManager.EmitCommand(command);
        }

        private void OnDeleteSessionResponse(OrchestratorCommand command, OrchestratorResponse response)
        {
            ResponseStatus status = new ResponseStatus(response.error, response.message);
            if (ResponsesListener != null) ResponsesListener.OnDeleteSessionResponse(status);
        }

        public bool JoinSession(string sessionId)
        {
            OrchestratorCommand command = GetOrchestratorCommand("JoinSession");
            command.GetParameter("sessionId").ParamValue = sessionId;
            return OrchestrationSocketIoManager.EmitCommand(command);
        }

        private void OnJoinSessionResponse(OrchestratorCommand command, OrchestratorResponse response)
        {
            ResponseStatus status = new ResponseStatus(response.error, response.message);
            if (ResponsesListener != null) ResponsesListener.OnJoinSessionResponse(status);
        }

        public bool LeaveSession()
        {
            OrchestratorCommand command = GetOrchestratorCommand("LeaveSession");
            return OrchestrationSocketIoManager.EmitCommand(command);
        }

        private void OnLeaveSessionResponse(OrchestratorCommand command, OrchestratorResponse response)
        {
            ResponseStatus status = new ResponseStatus(response.error, response.message);
            if (ResponsesListener != null) ResponsesListener.OnLeaveSessionResponse(status);
        }

        public bool GetScenarios()
        {
            OrchestratorCommand command = GetOrchestratorCommand("GetScenarios");
            return OrchestrationSocketIoManager.EmitCommand(command);
        }

        private void OnGetScenariosResponse(OrchestratorCommand command, OrchestratorResponse response)
        {
            ResponseStatus status = new ResponseStatus(response.error, response.message);
            List<Scenario> list = Helper.ParseElementsList<Scenario>(response.body);
            if (ResponsesListener != null) ResponsesListener.OnGetScenariosResponse(status, list);
        }

        public bool GetScenarioInstanceInfo(string scenarioId)
        {
            OrchestratorCommand command = GetOrchestratorCommand("GetScenarioInstanceInfo");
            command.GetParameter("scenarioId").ParamValue = scenarioId;
            return OrchestrationSocketIoManager.EmitCommand(command);
        }

        private void OnGetScenarioInstanceInfoResponse(OrchestratorCommand command, OrchestratorResponse response)
        {
            ResponseStatus status = new ResponseStatus(response.error, response.message);
            ScenarioInstance scenario = ScenarioInstance.ParseJsonData<ScenarioInstance>(response.body);
            if (ResponsesListener != null) ResponsesListener.OnGetScenarioInstanceInfoResponse(status, scenario);
        }

        public bool GetUsers()
        {
            OrchestratorCommand command = GetOrchestratorCommand("GetUsers");
            return OrchestrationSocketIoManager.EmitCommand(command);
        }

        private void OnGetUsersResponse(OrchestratorCommand command, OrchestratorResponse response)
        {
            ResponseStatus status = new ResponseStatus(response.error, response.message);
            List<User> list = Helper.ParseElementsList<User>(response.body);
            if (ResponsesListener != null) ResponsesListener.OnGetUsersResponse(status, list);
        }

        public bool AddUser(string userName, string userPassword, bool isAdmin)
        {
            OrchestratorCommand command = GetOrchestratorCommand("AddUser");
            command.GetParameter("userName").ParamValue = userName;
            command.GetParameter("userPassword").ParamValue = userPassword;
            command.GetParameter("userAdmin").ParamValue = isAdmin;
            return OrchestrationSocketIoManager.EmitCommand(command);
        }

        private void OnAddUserResponse(OrchestratorCommand command, OrchestratorResponse response)
        {
            ResponseStatus status = new ResponseStatus(response.error, response.message);
            User user = User.ParseJsonData<User>(response.body);
            if (ResponsesListener != null) ResponsesListener.OnAddUserResponse(status, user);
        }

        public bool GetUserInfo(string userId)
        {
            OrchestratorCommand command = GetOrchestratorCommand("GetUserInfo");
            command.GetParameter("userId").ParamValue = userId;
            return OrchestrationSocketIoManager.EmitCommand(command);
        }

        private void OnGetUserInfoResponse(OrchestratorCommand command, OrchestratorResponse response)
        {
            ResponseStatus status = new ResponseStatus(response.error, response.message);
            User user = User.ParseJsonData<User>(response.body);
            if (ResponsesListener != null) ResponsesListener.OnGetUserInfoResponse(status, user);
        }

        public bool GetUserData(string userId)
        {
            OrchestratorCommand command = GetOrchestratorCommand("GetUserData");
            command.GetParameter("userId").ParamValue = userId;
            return OrchestrationSocketIoManager.EmitCommand(command);
        }

        private void OnGetUserDataResponse(OrchestratorCommand command, OrchestratorResponse response)
        {
            ResponseStatus status = new ResponseStatus(response.error, response.message);
            UserData userData = UserData.ParseJsonData<UserData>(response.body);
            if (ResponsesListener != null) ResponsesListener.OnGetUserDataResponse(status, userData);
        }

        public bool UpdateUserData(string userDataKey, string userDataValue)
        {
            OrchestratorCommand command = GetOrchestratorCommand("UpdateUserData");
            command.GetParameter("userDataKey").ParamValue = userDataKey;
            command.GetParameter("userDataValue").ParamValue = userDataValue;
            return OrchestrationSocketIoManager.EmitCommand(command);
        }

        private void OnUpdateUserDataResponse(OrchestratorCommand command, OrchestratorResponse response)
        {
            ResponseStatus status = new ResponseStatus(response.error, response.message);
            if (ResponsesListener != null) ResponsesListener.OnUpdateUserDataResponse(status);
        }

        public bool UpdateUserDataArray(string userMQname, string userMQurl)
        {
            UserData userData = new UserData(userMQname, userMQurl);
            //JsonData json = JsonUtility.ToJson(userData);

            string userDataObj = "{\"userMQexchangeName\":\"" + userData.userMQexchangeName + "\", \"userMQurl\":\"" + userData.userMQurl + "\"}";

            OrchestratorCommand command = GetOrchestratorCommand("UpdateUserDataArray");
            command.GetParameter("userDataArray").ParamValue = userDataObj;
            return OrchestrationSocketIoManager.EmitCommand(command);
        }

        private void OnUpdateUserDataArrayResponse(OrchestratorCommand command, OrchestratorResponse response)
        {
            ResponseStatus status = new ResponseStatus(response.error, response.message);
        }

        public bool DeleteUser(string userId)
        {
            OrchestratorCommand command = GetOrchestratorCommand("DeleteUser");
            command.GetParameter("userId").ParamValue = userId;
            return OrchestrationSocketIoManager.EmitCommand(command);
        }

        private void OnDeleteUserResponse(OrchestratorCommand command, OrchestratorResponse response)
        {
            ResponseStatus status = new ResponseStatus(response.error, response.message);
            if (ResponsesListener != null) ResponsesListener.OnDeleteUserResponse(status);
        }

        public bool GetRooms()
        {
            OrchestratorCommand command = GetOrchestratorCommand("GetRooms");
            return OrchestrationSocketIoManager.EmitCommand(command);
        }

        private void OnGetRoomsResponse(OrchestratorCommand command, OrchestratorResponse response)
        {
            ResponseStatus status = new ResponseStatus(response.error, response.message);
            List<RoomInstance> rooms = Helper.ParseElementsList<RoomInstance>(response.body);
            if (ResponsesListener != null) ResponsesListener.OnGetRoomsResponse(status, rooms);
        }

        public bool JoinRoom(string roomId)
        {
            OrchestratorCommand command = GetOrchestratorCommand("JoinRoom");
            command.GetParameter("roomId").ParamValue = roomId;
            return OrchestrationSocketIoManager.EmitCommand(command);
        }

        private void OnJoinRoomResponse(OrchestratorCommand command, OrchestratorResponse response)
        {
            ResponseStatus status = new ResponseStatus(response.error, response.message);
            if (ResponsesListener != null) ResponsesListener.OnJoinRoomResponse(status);
        }

        public bool LeaveRoom()
        {
            OrchestratorCommand command = GetOrchestratorCommand("LeaveRoom");
            return OrchestrationSocketIoManager.EmitCommand(command);
        }

        private void OnLeaveRoomResponse(OrchestratorCommand command, OrchestratorResponse response)
        {
            ResponseStatus status = new ResponseStatus(response.error, response.message);
            if (ResponsesListener != null) ResponsesListener.OnLeaveRoomResponse(status);
        }

        public bool SendMessage(string message, string userId)
        {
            OrchestratorCommand command = GetOrchestratorCommand("SendMessage");
            command.GetParameter("message").ParamValue = message;
            command.GetParameter("userId").ParamValue = userId;
            return OrchestrationSocketIoManager.EmitCommand(command);
        }

        private void OnSendMessageResponse(OrchestratorCommand command, OrchestratorResponse response)
        {
            ResponseStatus status = new ResponseStatus(response.error, response.message);
            if (ResponsesListener != null) ResponsesListener.OnSendMessageResponse(status);
        }

        public bool SendMessageToAll(string message)
        {
            OrchestratorCommand command = GetOrchestratorCommand("SendMessageToAll");
            command.GetParameter("message").ParamValue = message;
            return OrchestrationSocketIoManager.EmitCommand(command);
        }

        private void OnSendMessageToAllResponse(OrchestratorCommand command, OrchestratorResponse response)
        {
            ResponseStatus status = new ResponseStatus(response.error, response.message);
            if (ResponsesListener != null) ResponsesListener.OnSendMessageToAllResponse(status);
        }
        #endregion


        #region grammar definition
        // declare te available commands, their parameters and the callbacks that should be used for the response of each command
        public void InitGrammar()
        {
            orchestratorCommands = new List<OrchestratorCommand>
                {              
                    //Login & Logout
                    new OrchestratorCommand("Login", new List<Parameter>
                        {
                            new Parameter("userName", typeof(string)),
                            new Parameter("userPassword", typeof(string))
                        },
                        OnLoginResponse),
                    new OrchestratorCommand("Logout", null, OnLogoutResponse),

                    ////orchestrator (not to use)
                    //new OrchestratorCommand("GetOrchestrator", "GetOrchestrator", null, OnOrchestratorResponse),

                    //sessions
                    new OrchestratorCommand("AddSession", new List<Parameter>
                        {
                            new Parameter("scenarioId", typeof(string)),
                            new Parameter("sessionName", typeof(string)),
                            new Parameter("sessionDescription", typeof(string))
                        },
                        OnAddSessionResponse),
                    new OrchestratorCommand("GetSessions", null, OnGetSessionsResponse),
                    new OrchestratorCommand("GetSessionInfo", null, OnGetSessionInfoResponse),
                    new OrchestratorCommand("DeleteSession", new List<Parameter>
                        {
                            new Parameter("sessionId", typeof(string)),
                        },
                        OnDeleteSessionResponse),
                    new OrchestratorCommand("JoinSession", new List<Parameter>
                        {
                            new Parameter("sessionId", typeof(string)),
                        },
                        OnJoinSessionResponse),
                    new OrchestratorCommand("LeaveSession", null, OnLeaveSessionResponse),

                    // scenarios
                    new OrchestratorCommand("GetScenarios", null, OnGetScenariosResponse),
                    new OrchestratorCommand("GetScenarioInstanceInfo", new List<Parameter>
                        {
                            new Parameter("scenarioId", typeof(string))
                        },
                        OnGetScenarioInstanceInfoResponse),

                    // users
                    new OrchestratorCommand("GetUsers", null, OnGetUsersResponse),
                    new OrchestratorCommand("GetUserInfo",
                    new List<Parameter>
                        {
                            new Parameter("userId", typeof(string))
                        },
                        OnGetUserInfoResponse),
                    new OrchestratorCommand("AddUser", new List<Parameter>
                        {
                            new Parameter("userName", typeof(string)),
                            new Parameter("userPassword", typeof(string)),
                            new Parameter("userAdmin", typeof(bool))
                        },
                        OnAddUserResponse),
                    new OrchestratorCommand("GetUserData",
                    new List<Parameter>
                        {
                            new Parameter("userId", typeof(string))
                        },
                        OnGetUserDataResponse),
                    new OrchestratorCommand("UpdateUserData", new List<Parameter>
                        {
                            new Parameter("userDataKey", typeof(string)),
                            new Parameter("userDataValue", typeof(string)),
                        },
                        OnUpdateUserDataResponse),
                     new OrchestratorCommand("UpdateUserDataArray", new List<Parameter>
                        {
                            new Parameter("userDataArray", typeof(string[])),
                        },
                        OnUpdateUserDataArrayResponse),
                    new OrchestratorCommand("DeleteUser", new List<Parameter>
                        {
                            new Parameter("userId", typeof(string))
                        },
                        OnDeleteUserResponse),

                    // rooms
                    new OrchestratorCommand("GetRooms", null, OnGetRoomsResponse),
                    //new OrchestratorCommand("GetRoomInfo", "GetRoomInfo", null),
                    new OrchestratorCommand("JoinRoom", new List<Parameter>
                        {
                            new Parameter("roomId", typeof(string))
                        },
                        OnJoinRoomResponse),
                    new OrchestratorCommand("LeaveRoom", null, OnLeaveRoomResponse),

                    //// NOTE: not to be done here: those messages are initiated by the orchestrator
                    //// new OrchestratorCommand("UpdateSession", "UpdateSession", null),
                    //// new OrchestratorCommand("SessionClosed", "SessionClosed", null),

                    ////messages TODO
                    new OrchestratorCommand("SendMessage", new List<Parameter>
                        {
                            new Parameter("message", typeof(string)),
                            new Parameter("userId", typeof(string))
                        },
                        OnSendMessageResponse),
                    new OrchestratorCommand("SendMessageToAll", new List<Parameter>
                        {
                            new Parameter("message", typeof(string))
                        },
                        OnSendMessageToAllResponse),
                    //new OrchestratorCommand("MessageSent", "MessageSent", null)
                };
        }

        // To retrieve the definition of a command by name
        public OrchestratorCommand GetOrchestratorCommand(string commandName)
        {
            for (int i = 0; i < orchestratorCommands.Count; i++)
            {
                if (orchestratorCommands[i].SocketEventName == commandName)
                {
                    return orchestratorCommands[i];
                }
            }
            return null;
        }
        #endregion
    }
}
