using AutonoFit.Classes;
using AutonoFit.Data;
using AutonoFit.Models;
using AutonoFit.Contracts;
using AutonoFit.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using Xunit;

namespace AutonoTest
{
    public class UnitTest1
    {
        [Fact]
        public void TestRemoveRepeats()
        {
            //Arrange
            int collectionSize = 3;
            List<Exercise> exercises = new List<Exercise> { };
            for(int i = 0; i < collectionSize; i++)
            {
                exercises.Add(new Exercise());
                exercises[i].exerciseId = i;
            }
            exercises[collectionSize - 1].exerciseId = 0;
            
            //Act
            exercises = SharedUtility.RemoveRepeats(exercises);

            //Assert
            Assert.NotEmpty(exercises);
            Assert.NotEqual(collectionSize, exercises.Count);
            Assert.True(exercises[0].exerciseId != exercises[1].exerciseId && exercises.Count <= 2);
        }

        [Fact]
        public void TestCheckLiftProgression()
        {
            //Arrange
            var strength = new Strength();
            List<Exercise> pastExercises = new List<Exercise> { };
            for(int i = 0; i < 3; i++)
            {
                pastExercises.Add(new Exercise());
                pastExercises[i].Reps = strength.minReps;
                pastExercises[i].RestSeconds = strength.maxRestSeconds;
                pastExercises[i].RPE = 12;
            }
            pastExercises[0].RPE = 10;
            Exercise newExercise = new Exercise();

            //Act
            LiftPrescription liftPrescription = new LiftPrescription();
            liftPrescription.CheckLiftProgression(pastExercises, strength, newExercise);

            //Assert
            Assert.Equal(newExercise.Reps, strength.minReps + strength.repsInterval);
            Assert.Equal(newExercise.RestSeconds, strength.maxRestSeconds - strength.restInterval);
        }

        [Fact]
        public void TestSelectOneExercise()
        {
            //Arrange
            List<Exercise> totalExercises = new List<Exercise> ();
            for (int i = 0; i < 3; i++)
            {
                totalExercises.Add(new Exercise());
                totalExercises[i].exerciseId = i;
            }

            List<Exercise> previouslyPerformed = new List<Exercise>();
            for(int i = 0; i < 2; i++)
            {
                previouslyPerformed.Add(new Exercise());
                previouslyPerformed[i].exerciseId = i;
                previouslyPerformed[i].description = null;//mimics the way it's stored in dB.
            }

            int liftWorkoutMinutes = 8;
            int availableMinutes = liftWorkoutMinutes;
            
            //Act
            LiftPrescription liftPrescript = new LiftPrescription();
            var exercise1 = liftPrescript.SelectOneExercise(totalExercises, previouslyPerformed, availableMinutes, liftWorkoutMinutes);
            availableMinutes /= 2;
            var exercise2 = liftPrescript.SelectOneExercise(totalExercises, previouslyPerformed, availableMinutes, liftWorkoutMinutes);

            //Assert
            Assert.NotNull(exercise1);
            Assert.NotNull(exercise2);
            Assert.NotEqual(exercise1, exercise2);
            Assert.DoesNotContain(exercise1, totalExercises);
            Assert.DoesNotContain(exercise2, totalExercises);
            Assert.DoesNotContain(exercise1, previouslyPerformed);
        }

        [Fact]
        public void TestSetBodyPartsForLiftingGoal()
        {
            //Arrange
            string expected = "Lower Body";
            bool todaysGoal = false;
            string lastWorkoutBodyParts = "Upper Body";
            bool supplementalLift = false;
            CardioComponent cardioComponent = null;
            //Act
            LiftPrescription liftPrescript = new LiftPrescription();
            string bodyParts = liftPrescript.SetBodyParts(lastWorkoutBodyParts, todaysGoal, supplementalLift, cardioComponent?.runType);
            //Assert
            Assert.Equal(bodyParts, expected);
        }
    }
}
