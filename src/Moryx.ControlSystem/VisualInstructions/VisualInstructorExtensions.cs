﻿using System;
using System.Reflection;
using Moryx.AbstractionLayer;
using Moryx.ControlSystem.Cells;

namespace Moryx.ControlSystem.VisualInstructions
{
    /// <summary>
    /// Extensions for <see cref="IVisualInstructor"/>
    /// </summary>
    public static class VisualInstructorExtensions
    {
        /// <summary>
        /// Only display these instructions
        /// Have to be cleared with the <see cref="IVisualInstructor.Clear"/> method
        /// </summary>
        public static long Display(this IVisualInstructor instructor, string sender, IVisualInstructions parameter)
        {
            return instructor.Display(sender, parameter.Instructions);
        }

        /// <summary>
        /// Only display these instructions
        /// Instruction will automatically cleared after the given time
        /// </summary>
        public static void Display(this IVisualInstructor instructor, string sender, IVisualInstructions parameter, int autoClearMs)
        {
            instructor.Display(sender, parameter.Instructions, autoClearMs);
        }

        /// <summary>
        /// Display the instructions on an activity
        /// </summary>
        public static long Display(this IVisualInstructor instructor, string sender, ActivityStart activityStart)
        {
            var instructions = GetInstructions(activityStart);
            return instructor.Display(sender, instructions);
        }

        /// <summary>
        /// Execute these instructions based on the given activity and report the result on completion
        /// Can (but must not) be cleared with the <see cref="IVisualInstructor.Clear"/> method
        /// </summary>
        public static long Execute(this IVisualInstructor instructor, string sender, IVisualInstructions parameter, IInstructionResults results)
        {
            return instructor.Execute(sender, parameter.Instructions, results);
        }

        /// <summary>
        /// Execute the instructions of an activity
        /// </summary>
        public static long Execute(this IVisualInstructor instructor, string sender, ActivityStart activityStart, Action<int, ActivityStart> callback)
        {
            var instructions = GetInstructions(activityStart);
            return Execute(instructor, sender, activityStart, callback, instructions);
        }

        /// <summary>
        /// Executes an instruction based on a activity session (<see cref="ActivityStart"/>).
        /// Parameters can be set manually
        /// </summary>
        public static long Execute(this IVisualInstructor instructor, string sender, ActivityStart activityStart, Action<int, ActivityStart> callback, IVisualInstructions parameters)
        {
            return Execute(instructor, sender, activityStart, callback, parameters.Instructions);
        }

        /// <summary>
        /// Executes an instruction based on a activity session (<see cref="ActivityStart"/>).
        /// Parameters can be set manually
        /// </summary>
        public static long Execute(this IVisualInstructor instructor, string sender, ActivityStart activityStart, Action<int, ActivityStart> callback, VisualInstruction[] parameters)
        {
            var activity = activityStart.Activity;

            var attr = activity.GetType().GetCustomAttribute<ActivityResultsAttribute>();
            if (attr == null)
                throw new ArgumentException($"Activity is not decorated with the {nameof(ActivityResultsAttribute)}");

            if (!attr.ResultEnum.IsEnum)
                throw new ArgumentException("Result type is not an enum!");

            var results = new ActivityInstructionResult(attr.ResultEnum, callback, activityStart);
            return instructor.Execute(sender, parameters, results);
        }

        private static VisualInstruction[] GetInstructions(ActivityStart activity)
        {
            var parameters = ((IActivity<IParameters>)activity.Activity).Parameters as IVisualInstructions;
            if (parameters == null)
                throw new ArgumentException($"Activity parameters are not of type {nameof(IVisualInstructions)}.");

            return parameters.Instructions;
        }
    }
}