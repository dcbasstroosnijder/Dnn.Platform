﻿// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Dnn.DynamicContent;
using Dnn.Tests.DynamicContent.UnitTests.Builders;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Dnn.DynamicContent.Repositories;

namespace Dnn.Tests.DynamicContent.UnitTests
{
    [TestFixture]
    public class FieldDefinitionCheckerTests
    {

        private MockRepository _mockRepository;
        private Mock<IFieldDefinitionRepository> _mockFieldDefinitionRepository;
        private Mock<IDynamicContentTypeManager> _mockDynamicContentTypeManager;

        [SetUp]
        public void Setup()
        {
            //Mock Repository
            _mockRepository = new MockRepository(MockBehavior.Default);

            // Setup Mock
            _mockFieldDefinitionRepository = _mockRepository.Create<IFieldDefinitionRepository>();
            _mockDynamicContentTypeManager = _mockRepository.Create<IDynamicContentTypeManager>();
            FieldDefinitionRepository.SetTestableInstance(_mockFieldDefinitionRepository.Object);
            DynamicContentTypeManager.SetTestableInstance(_mockDynamicContentTypeManager.Object);
        }

        [TearDown]
        public void TearDown()
        {
            FieldDefinitionRepository.ClearInstance();
            DynamicContentTypeManager.ClearInstance();
        }

        [Test]
        public void IsValid_Returns_False_If_NonExistingDynamicContentType()
        {
            //Arrange
            var baseFieldDefinition = new FieldDefinitionBuilder().WithName("TestField").Build();
            _mockDynamicContentTypeManager.Setup(m => m.GetContentType(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).
                Returns(() => null);

            //Act 
            string errorMessage;
            var isValid = new FieldDefinitionChecker().IsValid(baseFieldDefinition, out errorMessage);

            //Assert
            Assert.IsFalse(isValid, "The field definition with an inexisting dynamic content type is valid");
            Assert.AreEqual("The content type to which the field definition belongs is not valid.", errorMessage, "Error message is not the expected one");
            _mockRepository.VerifyAll();
        }


        [Test]
        public void IsValid_Returns_False_If_TypeOfANonExistingDynamicContentType()
        {
            //Arrange
            var baseFieldDefinition = new FieldDefinitionBuilder().WithName("TestField").WithIsReferenceType(true).WithFieldTypeId(0).Build();
            _mockDynamicContentTypeManager.Setup(m => m.GetContentType(It.IsInRange(1, int.MaxValue, Range.Inclusive), It.IsAny<int>(), It.IsAny<bool>())).
                Returns(() => new DynamicContentTypeBuilder().Build());
            _mockDynamicContentTypeManager.Setup(m => m.GetContentType(0, It.IsAny<int>(), It.IsAny<bool>())).
                Returns(() => null);

            //Act 
            string errorMessage;
            var isValid = new FieldDefinitionChecker().IsValid(baseFieldDefinition, out errorMessage);

            //Assert
            Assert.IsFalse(isValid, "The field definition with an inexisting dynamic content type is valid");
            Assert.AreEqual("The specified content type is not valid.", errorMessage, "Error message is not the expected one");
            _mockRepository.VerifyAll();
        }

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        [TestCase(6)]
        [TestCase(7)]
        [TestCase(8)]
        [TestCase(9)]
        [TestCase(10)]
        public void IsValid_Returns_False_If_FieldDefinition_WithCircularReference(int depthLevel)
        {
            //Arrange
            var listFieldDefinitions = new List<FieldDefinition>();
            const string baseFieldName = "BaseField0";
            var baseFieldDefinition = new FieldDefinitionBuilder().WithName(baseFieldName).WithIsReferenceType(true).Build();

            var nestedFieldDefinition = baseFieldDefinition;
            for (var i = 0; i < depthLevel; i++)
            {
                var fieldName = "BaseField" + (i + 1);

                nestedFieldDefinition = new FieldDefinitionBuilder().WithName(fieldName).WithIsReferenceType(true).
                    WithFieldTypeId(nestedFieldDefinition.ContentTypeId).
                    WithFieldDefinitionId(nestedFieldDefinition.FieldDefinitionId + 1).
                    WithContentTypeId(nestedFieldDefinition.ContentTypeId + 1).Build();
                
                listFieldDefinitions.Add(nestedFieldDefinition);
            }
            baseFieldDefinition.FieldTypeId = nestedFieldDefinition.ContentTypeId;
            listFieldDefinitions.Add(baseFieldDefinition);

            var fieldDefinitionCounter = listFieldDefinitions.Count - 1;
            if (depthLevel > 0)
            {
                _mockFieldDefinitionRepository.Setup(m => m.Get(It.IsAny<int>()))
                    .Returns(() => new List<FieldDefinition> {listFieldDefinitions[fieldDefinitionCounter]}.AsQueryable())
                    .Callback(() => fieldDefinitionCounter--);
            }

            _mockDynamicContentTypeManager.Setup(m => m.GetContentType(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).
                Returns(() => new DynamicContentTypeBuilder().Build());

            //Act 
            string errorMessage;
            var isValid = new FieldDefinitionChecker().IsValid(baseFieldDefinition, out errorMessage);

            //Assert
            Assert.IsFalse(isValid, "The field definition containing a dead loop is valid");
            Assert.AreEqual(string.Format("It is not posible to create the field with name {0} because it would create a dead loop definition.",
                baseFieldName), errorMessage, "Error message is not the expected one");
            _mockRepository.VerifyAll();
        }
    }
}
