using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using MoonSharp.Interpreter;
using IronPython.Hosting;

using ClientCommon.Data.InformationContext;
using Common.Models;

using ClientCommon.ClientService.ScriptUtilities;

namespace ClientCommon.ClientService
{
    public class VerificationService : BackgroundService
    {
        private readonly ILogger<VerificationService> _logger;
        private readonly IScriptProvider _scriptProvider;
        private readonly IClientInformationContext _informationContext;

        public VerificationService(ILogger<VerificationService> logger, IScriptProvider scriptProvider, IClientInformationContext informationContext)
        {
            _logger = logger;
            _scriptProvider = scriptProvider;
            _informationContext = informationContext;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            UserData.RegisterType(_scriptProvider.GetType());
            UserData.RegisterAssembly();

            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(1500, cancellationToken);

                await VerifySystem(cancellationToken);
            }
        }

        public async Task VerifySystem(CancellationToken cancellationToken = default)
        {
            try
            {
                var teamInfo = await _informationContext.GetTeam(cancellationToken);
                var system = await _informationContext.GetSystem(cancellationToken);

                await Task.WhenAll(
                    Task.Run(async () =>
                    {
                        var completedTasks = new List<CompetitionTask>();

                        foreach (var task in system.CompetitionTasks)
                        {
                            if (await IsTaskCompleted(task, cancellationToken))
                            {
                                completedTasks.Add(task);
                            }
                        }

                        await _informationContext.SetCompletedTasksAsync(completedTasks, cancellationToken);
                    }, cancellationToken),

                    Task.Run(async () =>
                    {
                        var appliedPenalties = new List<CompetitionPenalty>();

                        foreach (var penalty in system.CompetitionPenalties)
                        {
                            if (await DoesPenaltyApply(penalty, cancellationToken))
                            {
                                appliedPenalties.Add(penalty);
                            }
                        }

                        await _informationContext.SetAppliedPenaltiesAsync(appliedPenalties);
                    }, cancellationToken)
                );
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Could not verify system");
            }
        }

        private Task<bool> DoesPenaltyApply(CompetitionPenalty penalty, CancellationToken cancellationToken = default)
        {
            bool DoesLuaPenaltyApply(string scriptCode)
            {
                var script = new Script();
                var Penalty = new LuaPenalty();

                script.Globals.Set("Penalty", UserData.Create(Penalty));
                script.Globals.Set("Env", UserData.Create(_scriptProvider));

                try
                {
                    DynValue result = script.DoString(scriptCode);
                }
                catch (FinishExecutionException)
                {
                }

                return Penalty.DoesApply;
            }

            bool DoesPythonPenaltyApply(string script)
            {
                var engine = Python.CreateEngine();
                var scope = engine.CreateScope();
                var source = engine.CreateScriptSourceFromString(script);
                var penalty = new PythonPenalty();

                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    engine.Runtime.LoadAssembly(assembly);
                }

                scope.SetVariable("Penalty", penalty);
                scope.SetVariable("Env", _scriptProvider);

                try
                {
                    var result = source.Execute(scope);
                }
                catch (FinishExecutionException)
                {
                }

                return penalty.DoesApply;
            }

            return penalty.ScriptType switch
            {
                ScriptType.Lua => Task.Run(() => DoesLuaPenaltyApply(penalty.PenaltyScript), cancellationToken),
                ScriptType.Python => Task.Run(() => DoesPythonPenaltyApply(penalty.PenaltyScript), cancellationToken),
                _ => throw new Exception("Invalid script type")
            };
        }

        private Task<bool> IsTaskCompleted(CompetitionTask task, CancellationToken cancellationToken = default)
        {
            bool IsLuaTaskCompleted(string scriptCode)
            {
                var script = new Script();
                var Task = new LuaTask();

                script.Globals.Set("Task", UserData.Create(Task));
                script.Globals.Set("Env", UserData.Create(_scriptProvider));

                try
                {
                    DynValue result = script.DoString(scriptCode);
                }
                catch (FinishExecutionException)
                {
                }

                return Task.IsCompleted;
            }

            bool IsPythonTaskCompleted(string scriptCode)
            {
                var engine = Python.CreateEngine();
                var scope = engine.CreateScope();
                var source = engine.CreateScriptSourceFromString(scriptCode);
                var task = new PythonTask();

                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    engine.Runtime.LoadAssembly(assembly);
                }

                scope.SetVariable("Penalty", task);
                scope.SetVariable("Env", _scriptProvider);

                try
                {
                    var result = source.Execute(scope);
                }
                catch (FinishExecutionException)
                {
                }

                return task.IsCompleted;
            }

            return task.ScriptType switch
            {
                ScriptType.Lua => Task.Run(() => IsLuaTaskCompleted(task.ValidationScript), cancellationToken),
                ScriptType.Python => Task.Run(() => IsPythonTaskCompleted(task.ValidationScript), cancellationToken),
                _ => throw new Exception("Unknown script type")
            };
        }
    }
}
