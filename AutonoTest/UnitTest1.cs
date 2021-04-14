using AutonoFit.Classes;
using AutonoFit.Data;
using AutonoFit.Models;
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
    }
}
