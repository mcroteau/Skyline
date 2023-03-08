using System;
using System.Text;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using Zeus;

namespace Zeus{

    public class ExperienceResolver {

        int ZERO = 0;
        int ONE  = 1;

        String DOT      = "\\.";
        String NEWLINE  = "\n";
        String LOCATOR  = "\\$\\{[a-zA-Z+\\.+\\(\\a-zA-Z+)]*\\}";
        String FOREACH  = "<a:foreach";
        String ENDEACH  = "</a:foreach>";
        String IFSPEC   = "<a:if";
        String ENDIF    = "</a:if>";
        String SETVAR   = "<a:set";
        String OPENSPEC = "<a:if spec=\"${";
        String ENDSPEC  = "}";

        String COMMENT      = "<%--";
        String HTML_COMMENT = "<!--";

        public String resolve(String pageElement, ViewCache viewCache, NetworkRequest req, SecurityAttributes securityAttributes, ArrayList viewRenderers) {
            String[] elementEntriesPre = pageElement.Split("\n");
            
            ArrayList elementEntries = new ArrayList();
            elementEntries.AddRange(elementEntriesPre);

            ArrayList viewRendererElementEntries = getInterpretedRenderers(req, securityAttributes, elementEntries, viewRenderers);

            ArrayList dataPartials = getConvertedDataPartials(viewRendererElementEntries);
            ArrayList dataPartialsInflated = getInflatedPartials(dataPartials, viewCache);

            ArrayList dataPartialsComplete = getCompletedPartials(dataPartialsInflated, viewCache);
            ArrayList dataPartialsCompleteReady = getNilElementEntryPartials(dataPartialsComplete);

            StringBuilder pageElementsComplete = getElementsCompleted(dataPartialsCompleteReady);
            return pageElementsComplete.ToString();
        }

        ArrayList getNilElementEntryPartials(ArrayList dataPartialsComplete) {
            ArrayList dataPartialsCompleteReady = new ArrayList();
            foreach(DataPartial dataPartial in dataPartialsComplete){
                String elementEntry = dataPartial.getEntry();
                Regex regexLocator = new Regex(LOCATOR, RegexOptions.IgnoreCase);
                Match matcher = regexLocator.Match(elementEntry);
                while (matcher.Success){
                    String element = matcher.Value;
                    String regexElement = element
                            .Replace("${", "\\$\\{")
                            .Replace("}", "\\}");

                    String elementEntryComplete = elementEntry.Replace(regexElement, "");
                    dataPartial.setEntry(elementEntryComplete);
                    matcher = matcher.NextMatch();
                }
                dataPartialsCompleteReady.Add(dataPartial);
            }
            return dataPartialsCompleteReady;
        }

        StringBuilder getElementsCompleted(ArrayList dataPartialsComplete) {
            StringBuilder builder = new StringBuilder();
            foreach(DataPartial dataPartial in dataPartialsComplete) {
                if(!dataPartial.getEntry().Equals("") &&
                        !dataPartial.getEntry().Contains(SETVAR)){
                    builder.Append(dataPartial.getEntry() + NEWLINE);
                }
            }
            return builder;
        }

        void setResponseVariable(String baseEntry, ViewCache resp) {
            int startVariableIdx = baseEntry.IndexOf("var=");
            int endVariableIdx = baseEntry.IndexOf("\"", startVariableIdx + 5);
            String variableEntry = baseEntry.Substring(startVariableIdx + 5, endVariableIdx);
            int startValueIdx = baseEntry.IndexOf("val=");
            int endValueIdx = baseEntry.IndexOf("\"", startValueIdx + 5);
            String valueEntry = baseEntry.Substring(startValueIdx + 5, endValueIdx);
            resp.set(variableEntry, valueEntry);
        }

        ArrayList getConvertedDataPartials(ArrayList elementEntries) {
            ArrayList dataPartials = new ArrayList();
            foreach (String elementEntry in elementEntries) {
                if(elementEntry.Contains(COMMENT) ||
                        elementEntry.Contains(HTML_COMMENT))continue;
                DataPartial dataPartial = new DataPartial();
                dataPartial.setEntry(elementEntry);
                if (elementEntry.Contains(this.FOREACH)) {
                    dataPartial.setIterable(true);
                }else if(elementEntry.Contains(this.IFSPEC)) {
                    dataPartial.setSpec(true);
                }else if(elementEntry.Contains(this.ENDEACH)){
                    dataPartial.setEndIterable(true);
                }else if(elementEntry.Contains(this.ENDIF)){
                    dataPartial.setEndSpec(true);
                }else if(elementEntry.Contains(SETVAR)){
                    dataPartial.setSetVar(true);
                }
                dataPartials.Add(dataPartial);
            }
            return dataPartials;
        }

        ArrayList getInterpretedRenderers(NetworkRequest req, SecurityAttributes securityAttributes, ArrayList elementEntries, ArrayList viewRenderers){

            foreach(String viewRendererKlass in viewRenderers){
                Console.WriteLine(viewRendererKlass);
                Object viewRendererInstance = Activator.CreateInstance(typeof(viewRendererKlass), new Object[]{});

                Console.WriteLine(viewRendererInstance);
                
                MethodInfo getKey = viewRendererInstance.GetType().GetMethod("getKey");
                String rendererKey = (String) getKey.Invoke(viewRendererInstance, new Object[]{});

                String openRendererKey = "<" + rendererKey + ">";
                String completeRendererKey = "<" + rendererKey + "/>";
                String endRendererKey = "</" + rendererKey + ">";

                for(int tao = 0; tao < elementEntries.Count; tao++) {

                    String elementEntry = (String) elementEntries[tao];
                    MethodInfo isEval = viewRendererInstance.GetType().GetMethod("isEval", new Type[]{});
                    MethodInfo truthy = viewRendererInstance.GetType().GetMethod("truthy", new Type[]{NetworkRequest.GetType(), SecurityAttributes.GetType()});

                    if (((Boolean)(isEval.Invoke(viewRendererInstance, new Object[]{}))) &&
                            elementEntry.Contains(openRendererKey) &&
                            truthy.Invoke(viewRendererInstance, new Object[]{req, securityAttributes})) {

                        for(int moa = tao; moa < elementEntries.Count; moa++){
                            String elementEntryDeux = (String) elementEntries[moa];
                            elementEntries[moa] = elementEntryDeux;
                            if(elementEntryDeux.Contains(endRendererKey))break;
                        }
                    }
                    if (((Boolean)isEval.Invoke(viewRendererInstance, new Object[]{})) &&
                            elementEntry.Contains(openRendererKey) &&
                            !truthy.Invoke(viewRendererInstance, new Object[]{req, securityAttributes})) {
                        for(int moa = tao; moa < elementEntries.Count; moa++){
                            String elementEntryDeux = (String) elementEntries[moa];
                            elementEntries[moa] = "";
                            if(elementEntryDeux.Contains(endRendererKey))break;
                        }
                    }
                    if(!(((Boolean)isEval.Invoke(viewRendererInstance, new Object[]{}))) &&
                            elementEntry.Contains(completeRendererKey)){
                        MethodInfo render = viewRendererInstance.GetType().GetMethod("render", new Type[] { NetworkRequest.GetType(), SecurityAttributes.GetType()});
                        String rendered = (String) render.Invoke(viewRendererInstance, new Object[]{req, securityAttributes});
                        elementEntries[tao] =  rendered;
                    }
                }
            }
            return elementEntries;
        }

        ArrayList getCompletedPartials(ArrayList dataPartialsPre, ViewCache resp) {

            ArrayList dataPartials = new ArrayList();
            foreach(DataPartial dataPartial in dataPartialsPre) {

                if (dataPartial.getSpecPartials().Count != 0) {
                    Boolean passesRespSpecsIterations = true;
                    Boolean passesObjectSpecsIterations = true;
                    
                    foreach(DataPartial specPartial in dataPartial.getSpecPartials()) {
                        if(dataPartial.getComponents().Count != 0){
                            foreach(ObjectComponent objectComponent in dataPartial.getComponents()) {
                                Object objectInstance = objectComponent.getInstance();
                                if (!passesSpec(objectInstance, specPartial, dataPartial, resp)) {
                                    passesObjectSpecsIterations = false;
                                    goto specIteration;
                                }
                            }
                        }else{
                            if (!passesSpec(specPartial, resp)){
                                passesRespSpecsIterations = false;
                            }
                        }
                    }

                    specIteration:;
                            
                    if(dataPartial.getComponents().Count == 0) {
                        if (passesRespSpecsIterations) {
                            String entryBase = dataPartial.getEntry();
                            if (!dataPartial.isSetVar()) {
                                ArrayList lineComponents = getPageLineComponents(entryBase);
                                String entryBaseComplete = getCompleteLineElementResponse(entryBase, lineComponents, resp);
                                DataPartial completePartial = new DataPartial(entryBaseComplete);
                                dataPartials.Add(completePartial);
                            } else {
                                setResponseVariable(entryBase, resp);
                            }
                        }
                    }else if (passesObjectSpecsIterations) {
                        if (dataPartial.getComponents().Count != 0) {
                            String entryBase = dataPartial.getEntry();
                            foreach(ObjectComponent objectComponent in dataPartial.getComponents()) {
                                Object objectInstance = objectComponent.getInstance();
                                String activeField = objectComponent.getActiveField();
                                if (!dataPartial.isSetVar()) {
                                    entryBase = getCompleteLineElementObject(activeField, objectInstance, entryBase, resp);
                                } else {
                                    setResponseVariable(entryBase, resp);
                                }
                            }
                            DataPartial completePartial = new DataPartial(entryBase);
                            dataPartials.Add(completePartial);
                        }
                    }

                }else if(dataPartial.isWithinIterable()){
                    String entryBase = dataPartial.getEntry();
                    if(!dataPartial.isSetVar()) {
                        String entryBaseComplete = getCompleteInflatedDataPartial(dataPartial, resp);
                        DataPartial completePartial = new DataPartial(entryBaseComplete);
                        dataPartials.Add(completePartial);
                    }else{
                        setResponseVariable(entryBase, resp);
                    }
                }else{
                    String entryBase = dataPartial.getEntry();
                    if(!dataPartial.isSetVar()) {
                        ArrayList lineComponents = getPageLineComponents(entryBase);
                        String entryBaseComplete = getCompleteLineElementResponse(entryBase, lineComponents, resp);
                        DataPartial completePartial = new DataPartial(entryBaseComplete);
                        dataPartials.Add(completePartial);
                    }else{
                        setResponseVariable(entryBase, resp);
                    }
                }
            }
            return dataPartials;
        }

        String getCompleteLineElementObject(String activeField, Object objectInstance, String entryBase, ViewCache resp){
            ArrayList lineComponents = getPageLineComponents(entryBase);
            ArrayList iteratedLineComponents = new ArrayList();
            foreach(LineComponent lineComponent in lineComponents){
                if(activeField.Equals(lineComponent.getActiveField())) {
                    String objectField = lineComponent.getObjectField();
                    String objectValue = getObjectValueForLineComponent(objectField, objectInstance);
                    if(objectValue != null){
                        String lineElement = lineComponent.getCompleteLineElement();
                        entryBase = entryBase.Replace(lineElement, objectValue);
                        lineComponent.setIterated(true);
                    }else{
                        lineComponent.setIterated(false);
                    }
                }
                iteratedLineComponents.Add(lineComponent);
            }

            String entryBaseComplete = getCompleteLineElementResponse(entryBase, iteratedLineComponents, resp);
            return entryBaseComplete;
        }

        String getCompleteInflatedDataPartial(DataPartial dataPartial, ViewCache resp){
            String entryBase = dataPartial.getEntry();
            ArrayList lineComponents = getPageLineComponents(entryBase);
            ArrayList iteratedLineComponents = new ArrayList();
            foreach(ObjectComponent objectComponent in dataPartial.getComponents()) {
                Object objectInstance = objectComponent.getInstance();
                String activeField = objectComponent.getActiveField();

                foreach(LineComponent lineComponent in lineComponents){
                    if(activeField.Equals(lineComponent.getActiveField())) {
                        String objectField = lineComponent.getObjectField();
                        String objectValue = getObjectValueForLineComponent(objectField, objectInstance);
                        if(objectValue != null){
                            String lineElement = lineComponent.getCompleteLineElement();
                            entryBase = entryBase.Replace(lineElement, objectValue);
                            lineComponent.setIterated(true);
                        }else{
                            lineComponent.setIterated(false);
                        }
                    }
                    iteratedLineComponents.Add(lineComponent);
                }
            }

            String entryBaseComplete = getCompleteLineElementResponse(entryBase, iteratedLineComponents, resp);
            return entryBaseComplete;
        }

        String getCompleteLineElementResponse(String entryBase, ArrayList lineComponents, ViewCache resp) {
            foreach(LineComponent lineComponent in lineComponents){
                String activeObjectField = lineComponent.getActiveField();
                String objectField = lineComponent.getObjectField();
                String objectValue = getResponseValueLineComponent(activeObjectField, objectField, resp);
                String objectValueClean = objectValue != null ? objectValue.Replace("${", "\\$\\{").Replace("}", "\\}") : "";
                if(objectValue != null && objectField.Contains("(")){
                    String lineElement = "\\$\\{" + lineComponent.getLineElement().Replace("(", "\\(").Replace(")", "\\)") + "\\}";
                    entryBase = entryBase.Replace(lineElement, objectValue);
                }else if(objectValue != null && !objectValue.Contains("(")){
                    String lineElement = lineComponent.getCompleteLineElement();
                    entryBase = entryBase.Replace(lineElement, objectValueClean);
                }
            }
            return entryBase;
        }


        ArrayList getInflatedPartials(ArrayList dataPartials, ViewCache resp){

            ArrayList activeObjectComponents = new ArrayList();
            ArrayList dataPartialsPre = new ArrayList();
            for(int tao = 0; tao < dataPartials.Count; tao++) {
                DataPartial dataPartial = (DataPartial) dataPartials[tao];
                String basicEntry = dataPartial.getEntry();

                DataPartial dataPartialSies = new DataPartial();
                dataPartialSies.setEntry(basicEntry);
                dataPartialSies.setSetVar(dataPartial.isSetVar());

                if(dataPartial.isIterable()) {
                    dataPartialSies.setWithinIterable(true);

                    IterableResult iterableResult = getIterableResult(basicEntry, resp);
                    ArrayList iterablePartials = getIterablePartials(tao + 1, dataPartials);

                    ArrayList objects = iterableResult.getMojos();//renaming variables
                    for(int foo = 0; foo < objects.Count; foo++){
                        Object objectInstance = objects[foo];

                        ObjectComponent component = new ObjectComponent();
                        component.setActiveField(iterableResult.getField());
                        component.setInstance(objectInstance);

                        ArrayList objectComponents = new ArrayList();
                        objectComponents.Add(component);

                        for(int beta = 0; beta < iterablePartials.Count; beta++){
                            DataPartial dataPartialDeux = (DataPartial) iterablePartials[beta];
                            String basicEntryDeux = dataPartialDeux.getEntry();

                            DataPartial dataPartialCinq = new DataPartial();
                            dataPartialCinq.setEntry(basicEntryDeux);
                            dataPartialCinq.setWithinIterable(true);
                            dataPartialCinq.setComponents(objectComponents);
                            dataPartialCinq.setField(iterableResult.getField());
                            dataPartialCinq.setSetVar(dataPartialDeux.isSetVar());

                            if(dataPartialDeux.isIterable()) {

                                IterableResult iterableResultDeux = getIterableResultNested(basicEntryDeux, objectInstance);
                                ArrayList iterablePartialsDeux = getIterablePartialsNested(beta + 1, iterablePartials);

                                ArrayList pojosDeux = iterableResultDeux.getMojos();

                                for(int tai = 0; tai < pojosDeux.Count; tai++){
                                    Object objectDeux = pojosDeux[tai];

                                    ObjectComponent componentDeux = new ObjectComponent();
                                    componentDeux.setActiveField(iterableResult.getField());
                                    componentDeux.setInstance(objectInstance);

                                    ObjectComponent componentTrois = new ObjectComponent();
                                    componentTrois.setActiveField(iterableResultDeux.getField());
                                    componentTrois.setInstance(objectDeux);

                                    ArrayList objectComponentsDeux = new ArrayList();
                                    objectComponentsDeux.Add(componentDeux);
                                    objectComponentsDeux.Add(componentTrois);

                                    for (int chi = 0; chi < iterablePartialsDeux.Count; chi++) {
                                        DataPartial dataPartialTrois = (DataPartial) iterablePartialsDeux[chi];
                                        DataPartial dataPartialQuatre = new DataPartial();
                                        dataPartialQuatre.setEntry(dataPartialTrois.getEntry());
                                        dataPartialQuatre.setWithinIterable(true);
                                        dataPartialQuatre.setComponents(objectComponentsDeux);
                                        dataPartialQuatre.setField(iterableResultDeux.getField());
                                        dataPartialQuatre.setSetVar(dataPartialTrois.isSetVar());

                                        if(!isPeripheralPartial(dataPartialTrois)) {
                                            ArrayList specPartials = getSpecPartials(dataPartialTrois, dataPartials);
                                            dataPartialQuatre.setSpecPartials(specPartials);
                                            dataPartialsPre.Add(dataPartialQuatre);
                                        }
                                    }
                                }
                            }else if(!isPeripheralPartial(dataPartialDeux) &&
                                    (isTrailingPartialNested(beta, iterablePartials) || !withinIterable(dataPartialDeux, iterablePartials))){
                                ArrayList specPartials = getSpecPartials(dataPartialDeux, dataPartials);
                                dataPartialCinq.setSpecPartials(specPartials);
                                dataPartialsPre.Add(dataPartialCinq);
                            }
                        }
                    }

                }else if(!isPeripheralPartial(dataPartial) &&
                        (isTrailingPartial(tao, dataPartials) || !withinIterable(dataPartial, dataPartials))){
                    ArrayList specPartials = getSpecPartials(dataPartial, dataPartials);
                    dataPartialSies.setComponents(activeObjectComponents);
                    dataPartialSies.setSpecPartials(specPartials);
                    dataPartialSies.setWithinIterable(false);
                    dataPartialsPre.Add(dataPartialSies);
                }
            }

            return dataPartialsPre;
        }

        Boolean isTrailingPartialNested(int tao, ArrayList dataPartials) {
            int openCount = 0, endCount = 0, endIdx = 0;
            for(int tai = 0; tai < dataPartials.Count; tai++){
                DataPartial dataPartial = (DataPartial) dataPartials[tai];
                if(dataPartial.isIterable())openCount++;
                if(dataPartial.isEndIterable()){
                    endCount++;
                    endIdx = tai;
                }
            }
            if(openCount == 1 && openCount == endCount && tao > endIdx)return true;
            return false;
        }

        Boolean isTrailingPartial(int chi, ArrayList dataPartials) {
            int openCount = 0, endIdx = 0;
            for(int tai = 0; tai < dataPartials.Count; tai++){
                DataPartial dataPartial = (DataPartial) dataPartials[tai];
                if(dataPartial.isIterable())openCount++;
                if(dataPartial.isEndIterable()){
                    endIdx = tai;
                }
            }
            if(openCount != 0 && chi > endIdx)return true;
            return false;
        }

        Boolean isPeripheralPartial(DataPartial dataPartial) {
            return dataPartial.isSpec() || dataPartial.isIterable() || dataPartial.isEndSpec() || dataPartial.isEndIterable();
        }

        ArrayList getSpecPartials(DataPartial dataPartialLocator, ArrayList dataPartials) {
            HashSet<DataPartial> specPartials = new HashSet<DataPartial>();
            for(int tao = 0; tao < dataPartials.Count; tao++){
                int openCount = 0; int endCount = 0;
                DataPartial dataPartial = (DataPartial) dataPartials[tao];
                if(dataPartial.isSpec()) {
                    openCount++;
                    for (int chao = tao; chao < dataPartials.Count; chao++) {
                        DataPartial dataPartialDeux = (DataPartial) dataPartials[chao];
                        if (dataPartialDeux.getGuid().Equals(dataPartial.getGuid())) continue; 
                        if (dataPartialLocator.getGuid().Equals(dataPartialDeux.getGuid())) {
                            goto matchIteration;
                        }
                        if (dataPartialDeux.isEndSpec()) {
                            endCount++;
                        }
                        if (dataPartialDeux.isSpec()) {
                            openCount++;
                        }
                        if(openCount == endCount)goto matchIteration;
                    }
                }
                matchIteration:;

                if(dataPartialLocator.getGuid().Equals(dataPartial.getGuid()))break;

                if(openCount > endCount)specPartials.Add(dataPartial);
            }
            ArrayList specPartialsReady = new ArrayList();
            foreach(DataPartial dataPartialComponent in specPartials){
                specPartialsReady.Add(dataPartialComponent);
            }

            return specPartialsReady;
        }

        ArrayList getIterablePartials(int openIdx, ArrayList dataPartials){
            int openCount = 1, endCount = 0;
            ArrayList dataPartialsDeux = new ArrayList();
            for (int foo = openIdx; foo < dataPartials.Count; foo++) {
                DataPartial dataPartial = (DataPartial) dataPartials[foo];

                if(dataPartial.isIterable())openCount++;
                if(dataPartial.isEndIterable())endCount++;

                if(openCount != 0 && openCount == endCount)break;

                dataPartialsDeux.Add(dataPartial);
            }
            return dataPartialsDeux;
        }

        ArrayList getIterablePartialsNested(int openIdx, ArrayList dataPartials){
            ArrayList dataPartialsDeux = new ArrayList();
            int endIdx = getEndEach(openIdx, dataPartials);
            for (int foo = openIdx; foo < endIdx; foo++) {
                DataPartial basePartial = (DataPartial) dataPartials[foo];
                dataPartialsDeux.Add(basePartial);
            }
            return dataPartialsDeux;
        }

        int getEndEach(int openIdx, ArrayList basePartials) {
            int openEach = 1;
            int endEach = 0;
            for (int qxro = openIdx + 1; qxro < basePartials.Count; qxro++) {
                DataPartial basePartial = (DataPartial) basePartials[qxro];
                String basicEntry = basePartial.getEntry();
                if(basicEntry.Contains(this.ENDEACH))endEach++;

                if(openEach > 3)throw new UnnamedException("too many nested <a:foreach>.");
                if(basicEntry.Contains(this.ENDEACH) && endEach == openEach && endEach != 0){
                    return qxro + 1;
                }
            }
            throw new UnnamedException("missing end </a:foreach>");
        }


        Boolean withinIterable(DataPartial dataPartial, ArrayList dataPartials){
            int openCount = 0, endCount = 0;
            foreach(DataPartial it in dataPartials){
                if(it.isIterable())openCount++;
                if(it.isEndIterable())endCount++;
                if(it.getGuid().Equals(dataPartial.getGuid()))break;
            }
            if(openCount == 1 && endCount == 0)return true;
            if(openCount == 2 && endCount == 1)return true;
            if(openCount == 2 && endCount == 0)return true;
            return false;
        }


        Boolean passesIterableSpec(DataPartial specPartial, Object activeObject, ViewCache resp){

            String specElementEntry = specPartial.getEntry();
            int startExpression = specElementEntry.IndexOf(OPENSPEC);
            int endExpression = specElementEntry.IndexOf(ENDSPEC);
            String expressionElement = specElementEntry.Substring(startExpression + OPENSPEC.Length, endExpression);
            String conditionalElement = getConditionalElement(expressionElement);

            if(conditionalElement.Equals(""))return false;

            String[] expressionElements = expressionElement.Split(conditionalElement);
            String subjectElement = expressionElements[ZERO].Trim();

            String[] subjectFieldElements = subjectElement.Split(DOT, 2);
            String activeSubjectFieldElement = subjectFieldElements[ZERO];
            String activeSubjectFieldsElement = subjectFieldElements[ONE];

            String predicateElement = expressionElements[ONE].Trim();
            Object activeSubjectObject = activeObject;

            String predicateValue = null;
            Boolean passesSpecification = false;
            if(activeSubjectFieldsElement.Contains("()")){
                String activeMethodName = activeSubjectFieldsElement.Replace("()", "");
                Object activeMethodObject = resp.get(activeSubjectFieldElement);
                if(activeMethodObject == null)return false;
                MethodInfo activeMethod = activeMethodObject.GetType().GetMethod(activeMethodName);
                Object activeObjectValue = activeMethod.Invoke(activeMethodObject, new Object[]{});
                if(activeObjectValue == null)return false;
                String subjectValueVar = (String)(activeObjectValue);
                int subjectNumericValue = Int32.Parse(subjectValueVar);
                predicateValue = predicateElement.Replace("'", "");
                int predicateNumericValue = Int32.Parse(predicateValue);
                passesSpecification = getValidation(subjectNumericValue, predicateNumericValue, conditionalElement, expressionElement);
                return passesSpecification;
            }else{
                String[] activeSubjectFieldElements = activeSubjectFieldsElement.Split(DOT);
                foreach(String activeFieldElement in activeSubjectFieldElements){
                    activeSubjectObject = getObjectValue(activeFieldElement, activeSubjectObject);
                }
            }

            String subjectValue = (String)(activeSubjectObject);

            if(predicateElement.Contains(".")){

                String[] predicateFieldElements = predicateElement.Split(DOT, 2);
                String predicateField = predicateFieldElements[ZERO];
                String activePredicateFields = predicateFieldElements[ONE];

                String[] activePredicateFieldElements = activePredicateFields.Split(DOT);
                Object activePredicateObject = resp.get(predicateField);
                if(activePredicateObject != null) {
                    foreach(String activeFieldElement in activePredicateFieldElements) {
                        activePredicateObject = getObjectValue(activeFieldElement, activePredicateObject);
                    }
                }

                predicateValue = (String)(activePredicateObject);
                passesSpecification = passesSpec(subjectValue, predicateValue, conditionalElement);
                return passesSpecification;

            }else if(!predicateElement.Contains("'")){
                Object activePredicateObject = resp.get(predicateElement);
                predicateValue = (String)(activePredicateObject);
                passesSpecification = passesSpec(subjectValue, predicateValue, conditionalElement);
                return passesSpecification;
            }

            predicateValue = predicateElement.Replace("'", "");
            passesSpecification = passesSpec(subjectValue, predicateValue, conditionalElement);
            return passesSpecification;
        }

        Boolean passesSpec(DataPartial specPartial, ViewCache resp) {
            String specElementEntry = specPartial.getEntry();
            int startExpression = specElementEntry.IndexOf(OPENSPEC);
            int endExpression = specElementEntry.IndexOf(ENDSPEC);
            String completeExpressionElement = specElementEntry.Substring(startExpression + OPENSPEC.Length, endExpression);

            String[] allElementExpressions = completeExpressionElement.Split("&&");
            
            String subjectField = new String("");
            String subjectValue = new String("");
            Object activeSubjectObject = new Object();
            String[] subjectFieldElements = new String[]{};
            String[] activeSubjectFieldElements = new String[]{};

            foreach(String expressionElementClean in allElementExpressions) {
                String expressionElement = expressionElementClean.Trim();
                String conditionalElement = getConditionalElement(expressionElement);

                String subjectElement = expressionElement, predicateElementClean = "";
                if (!conditionalElement.Equals("")) {
                    String[] expressionElements = expressionElement.Split(conditionalElement);
                    subjectElement = expressionElements[ZERO].Trim();
                    String predicateElement = expressionElements[ONE];
                    predicateElementClean = predicateElement.Replace("'", "").Trim();
                }

                if (subjectElement.Contains(".")) {

                    if (predicateElementClean.Equals("") &&
                            conditionalElement.Equals("")) {
                        Boolean falseActive = subjectElement.Contains("!");
                        String subjectElementClean = subjectElement.Replace("!", "");

                        subjectFieldElements = subjectElementClean.Split(DOT, 2);
                        subjectField = subjectFieldElements[ZERO];
                        String subjectFieldElementsRemainder = subjectFieldElements[ONE];

                        activeSubjectObject = resp.get(subjectField);
                        if (activeSubjectObject == null) return false;

                        activeSubjectFieldElements = subjectFieldElementsRemainder.Split(DOT);
                        foreach(String activeFieldElement in activeSubjectFieldElements) {
                            activeSubjectObject = getObjectValue(activeFieldElement, activeSubjectObject);
                        }

                        Boolean activeSubjectObjectBoolean = (Boolean) activeSubjectObject;
                        if (activeSubjectObjectBoolean && !falseActive) return true;
                        if (!activeSubjectObjectBoolean && falseActive) return true;
                    }

                    if (subjectElement.Contains("()")) {
                        String subjectElements = subjectElement.Replace("()", "");
                        subjectFieldElements = subjectElements.Split(DOT);
                        subjectField = subjectFieldElements[ZERO];
                        String methodName = subjectFieldElements[ONE];
                        activeSubjectObject = resp.get(subjectField);
                        if (activeSubjectObject == null) return false;
                        MethodInfo activeMethod = activeSubjectObject.GetType().GetMethod(methodName);
                        Object activeObjectValue = activeMethod.Invoke(activeSubjectObject, new Object[]{});
                        if (activeObjectValue == null) return false;
                        subjectValue = (String)(activeObjectValue);
                        int subjectNumericValue = Int32.Parse(subjectValue);
                        int predicateNumericValue = Int32.Parse(predicateElementClean);
                        if(getValidation(subjectNumericValue, predicateNumericValue, conditionalElement, expressionElement))return true;
                        return false;
                    }

                    subjectFieldElements = subjectElement.Split(DOT, 2);
                    subjectField = subjectFieldElements[ZERO];
                    String activeSubjectFields = subjectFieldElements[ONE];

                    activeSubjectFieldElements = activeSubjectFields.Split(DOT);
                    activeSubjectObject = resp.get(subjectField);
                    if (activeSubjectObject == null) return false;

                    foreach(String activeFieldElement in activeSubjectFieldElements) {
                        activeSubjectObject = getObjectValue(activeFieldElement, activeSubjectObject);
                    }

                    String[] expressionElements = expressionElement.Split(conditionalElement);
                    String predicateElement = expressionElements[ONE];

                    if(predicateElement.Contains("'")){
                        subjectValue = (String)(activeSubjectObject);
                        String predicateValue = predicateElement.Replace("'", "").Trim();
                        if(passesSpec(subjectValue, predicateValue, conditionalElement))return true;
                        return false;
                    }else{
                        String[] activePredicateFieldElements = predicateElement.Split(DOT);
                        String predicateField = activePredicateFieldElements[ZERO];
                        Object activePredicateObject = resp.get(predicateField);
                        if (activePredicateObject == null) return false;

                        foreach(String activeFieldElement in activePredicateFieldElements) {
                            activePredicateObject = getObjectValue(activeFieldElement, activePredicateObject);
                        }

                        subjectValue = (String)(activeSubjectObject);
                        String predicateValue = (String)(activeSubjectObject);

                        if (activeSubjectObject == null) {
                            if(!passesNilSpec(activeSubjectObject, predicateValue, conditionalElement))return false;
                        }

                        if(passesSpec(subjectValue, predicateValue, conditionalElement))return true;
                        return false;

                    }


                } else if (predicateElementClean.Equals("") &&
                        conditionalElement.Equals("")) {
                    Boolean falseActive = subjectElement.Contains("!");
                    String subjectElementClean = subjectElement.Replace("!", "");
                    activeSubjectObject = resp.get(subjectElementClean);
                    if (activeSubjectObject == null) return false;
                    Boolean activeSubjectObjectBoolean = (Boolean) activeSubjectObject;
                    if (!activeSubjectObjectBoolean && falseActive) return true;
                    if (activeSubjectObjectBoolean && !falseActive) return true;
                }

                if(!predicateElementClean.Equals("")) {
                    activeSubjectObject = resp.get(subjectElement);

                    if(!predicateElementClean.Contains(".") && activeSubjectObject == null) {
                        if (passesNilSpec(activeSubjectObject, predicateElementClean, conditionalElement)) return true;
                        return false;
                    }

                    String[] predicateFieldElements = predicateElementClean.Split(DOT, 2);
                    String predicateField = predicateFieldElements[ZERO];
                    String predicateFieldElementsRemainder = predicateFieldElements[ONE];

                    String[] activePredicateFieldElements = predicateFieldElementsRemainder.Split(DOT);
                    Object activePredicateObject = resp.get(predicateField);

                    foreach(String activeFieldElement in activePredicateFieldElements) {
                        activePredicateObject = getObjectValue(activeFieldElement, activePredicateObject);
                    }

                    subjectValue = ((String)activeSubjectObject).Trim();
                    String predicateValue = ((String)activePredicateObject).Trim();

                    if (activeSubjectObject == null) {
                        if (passesNilSpec(activeSubjectObject, predicateValue, conditionalElement)) return true;
                        return false;
                    }

                    if (passesSpec(subjectValue, predicateValue, conditionalElement)) return true;
                    return false;
                }

                activeSubjectObject = resp.get(subjectElement);
                subjectValue = ((String)activeSubjectObject).Trim();

                if (activeSubjectObject == null) {
                    if (!passesNilSpec(activeSubjectObject, predicateElementClean, conditionalElement)) return false;
                    return true;
                }

                if (passesSpec(subjectValue, predicateElementClean, conditionalElement)) return true;
                return false;
            }
            return true;
        }

        Boolean passesNilSpec(Object activeSubjectObject, Object activePredicateObject, String conditionalElement) {
            if(activeSubjectObject == null && activePredicateObject.Equals("null") && conditionalElement.Equals("=="))return true;
            if(activeSubjectObject == null && activePredicateObject.Equals("null") && conditionalElement.Equals("!="))return false;
            if(activeSubjectObject == null && activePredicateObject.Equals("") && conditionalElement.Equals("=="))return true;
            if(activeSubjectObject == null && activePredicateObject.Equals("") && conditionalElement.Equals(""))return false;
            if(activeSubjectObject == null && activePredicateObject.Equals("") && conditionalElement.Equals("!="))return false;
            return false;
        }

        Boolean passesSpec(String subjectValue, String predicateValue, String conditionalElement) {
            if(subjectValue.Equals(predicateValue) && conditionalElement.Equals("=="))return true;
            if(subjectValue.Equals(predicateValue) && conditionalElement.Equals("!="))return false;
            if(!subjectValue.Equals(predicateValue) && conditionalElement.Equals("!="))return true;
            if(!subjectValue.Equals(predicateValue) && conditionalElement.Equals("=="))return false;
            return false;
        }

        Boolean getValidation(int subjectValue, int predicateValue, String condition, String expressionElement){
            if(condition.Equals(">")) {
                if(subjectValue > predicateValue)return true;
            }else if (condition.Equals("==")) {
                if(subjectValue.Equals(predicateValue))return true;
            }
            return false;
        }

        String getConditionalElement(String expressionElement){
            if(expressionElement.Contains(">"))return ">";
            if(expressionElement.Contains("<"))return "<";
            if(expressionElement.Contains("=="))return "==";
            if(expressionElement.Contains(">="))return ">=";
            if(expressionElement.Contains("<="))return "<=";
            if(expressionElement.Contains("!="))return "!=";
            return "";
        }

        IterableResult getIterableResultNested(String entry, Object activeSubjectObject){
            int startEach = entry.IndexOf(this.FOREACH);

            int startIterate = entry.IndexOf("items=", startEach + 1);
            int endIterate = entry.IndexOf("\"", startIterate + 7);//items="
            String iterableKey = entry.Substring(startIterate + 9, endIterate -1 );//items="${ and }

            String iterablePadded = "${" + iterableKey + "}";

            int startField = iterablePadded.IndexOf(".");
            int endField = iterablePadded.IndexOf("}", startField);
            String activeSubjectFieldElement = iterablePadded.Substring(startField + 1, endField);

            int startItem = entry.IndexOf("var=", endIterate);
            int endItem = entry.IndexOf("\"", startItem + 5);//var="
            String activeField = entry.Substring(startItem + 5, endItem);

            String[] activeSubjectFieldElements = activeSubjectFieldElement.Split(DOT);
            foreach(String activeFieldElement in activeSubjectFieldElements){
                activeSubjectObject = getObjectValue(activeFieldElement, activeSubjectObject);
            }

            IterableResult iterableResult = new IterableResult();
            iterableResult.setField(activeField);
            iterableResult.setMojos((ArrayList) activeSubjectObject);
            return iterableResult;
        }

        private IterableResult getIterableResult(String entry, ViewCache httpResponse){

            int startEach = entry.IndexOf(this.FOREACH);

            int startIterate = entry.IndexOf("items=", startEach);
            int endIterate = entry.IndexOf("\"", startIterate + 7);//items=".
            String iterableKey = entry.Substring(startIterate + 9, endIterate -1 );//items="${ }

            int startItem = entry.IndexOf("var=", endIterate);
            int endItem = entry.IndexOf("\"", startItem + 6);//items="
            String activeField = entry.Substring(startItem + 5, endItem);

            String expression = entry.Substring(startIterate + 7, endIterate);

            ArrayList pojos = new ArrayList();
            if(iterableKey.Contains(".")){
                pojos = getIterableInitial(expression, httpResponse);
            }else if(httpResponse.getCache().ContainsKey(iterableKey)){
                pojos = (ArrayList) httpResponse.get(iterableKey);
            }

            IterableResult iterableResult = new IterableResult();
            iterableResult.setField(activeField);
            iterableResult.setMojos(pojos);
            return iterableResult;
        }

        ArrayList getIterableInitial(String expression, ViewCache httpResponse){
            int startField = expression.IndexOf("${");
            int endField = expression.IndexOf(".", startField);
            String key = expression.Substring(startField + 2, endField);
            if(httpResponse.getCache().ContainsKey(key)){
                Object obj = httpResponse.get(key);
                Object objList = getIterableRecursive(expression, obj);
                return (ArrayList) objList;
            }
            return new ArrayList();
        }

        ArrayList getIterableRecursive(String expression, Object activeSubjectObject) {
            ArrayList objs = new ArrayList();
            int startField = expression.IndexOf(".");
            int endField = expression.IndexOf("}");

            String activeSubjectFielElement = expression.Substring(startField + 1, endField);

            String[] activeSubjectFieldElements = activeSubjectFielElement.Split(DOT);
            foreach(String activeFieldElement in activeSubjectFieldElements){
                activeSubjectObject = getObjectValue(activeFieldElement, activeSubjectObject);
            }

            if(activeSubjectObject != null){
                return (ArrayList) activeSubjectObject;
            }
            return objs;
        }

        Object getIterableValueRecursive(int idx, String baseField, Object baseObj) {
            String[] fields = baseField.Split("\\.");
            if(fields.Length > 1){
                idx++;
                String key = fields[0];
                FieldInfo fieldObj = baseObj.GetType().GetField(key);
                if(fieldObj != null){
                    Object obj = fieldObj.GetValue(baseObj);
                    int start = baseField.IndexOf(".");
                    String fieldPre = baseField.Substring(start + 1);
                    if(obj != null) {
                        return getValueRecursive(idx, fieldPre, obj);
                    }
                }
            }else{
                FieldInfo fieldObj = baseObj.GetType().GetField(baseField);
                if(fieldObj != null) {
                    Object obj = fieldObj.GetValue(baseObj);
                    if (obj != null) {
                        return obj;
                    }
                }
            }
            return new ArrayList();
        }

        Object getValueRecursive(int idx, String baseField, Object baseObj) {
            String[] fields = baseField.Split("\\.");
            if(fields.Length > 1){
                idx++;
                String key = fields[0];
                FieldInfo fieldObj = baseObj.GetType().GetField(key);
                Object obj = fieldObj.GetValue(baseObj);
                int start = baseField.IndexOf(".");
                String fieldPre = baseField.Substring(start + 1);
                if(obj != null) {
                    return getValueRecursive(idx, fieldPre, obj);
                }

            }else{
                try {
                    FieldInfo fieldObj = baseObj.GetType().GetField(baseField);
                    Object obj = fieldObj.GetValue(baseObj);
                    if (obj != null) {
                        return obj;
                    }
                }catch(Exception ex){}
            }
            return null;
        }


        ArrayList getPageLineComponents(String pageElementEntry){
            ArrayList lineComponents = new ArrayList();
            Regex regexLocator = new Regex(LOCATOR, RegexOptions.IgnoreCase);
            Match matcher = regexLocator.Match(pageElementEntry);
            int matchCount = 0;
            while (matcher.Success){
                LineComponent lineComponent = new LineComponent();
                String lineElement = matcher.Value;
                String cleanElement = lineElement
                        .Replace("${", "")
                        .Replace("}", "");
                String activeField = cleanElement;
                String objectField = "";
                if(cleanElement.Contains(".")) {
                    String[] elements = cleanElement.Split("\\.", 2);
                    activeField = elements[0];
                    objectField = elements[1];
                }
                lineComponent.setActiveField(activeField);
                lineComponent.setObjectField(objectField);
                lineComponent.setLineElement(cleanElement);
                lineComponents.Add(lineComponent);
                matcher = matcher.NextMatch();
            }
            return lineComponents;
        }

        String getResponseValueLineComponent(String activeField, String objectField, ViewCache resp) {

            if(objectField.Contains(".")){

                Object activeObject = resp.get(activeField);
                if(activeObject != null) {

                    String[] activeObjectFields = objectField.Split(DOT);

                    foreach(String activeObjectField in activeObjectFields) {
                        activeObject = getObjectValue(activeObjectField, activeObject);
                    }

                    if (activeObject == null) return null;
                    return (String)(activeObject);

                }
            }else{

                Object respValue = resp.get(activeField);
                if(respValue != null &&
                        !objectField.Equals("") &&
                            !objectField.Contains(".")) {

                    Object objectValue = null;

                    if(objectField.Contains("()")){
                        String methodName = objectField.Replace("()", "");
                        MethodInfo methodObject = respValue.GetType().GetMethod(methodName);
                        if(methodObject != null) {
                            objectValue = methodObject.Invoke(respValue, new Object[]{});
                        }
                    }else if (isObjectMethod(respValue, objectField)) {

                        Object activeObject = getObjectMethodValue(resp, respValue, objectField);
                        if(activeObject == null) return null;

                        return (String)(activeObject);

                    }else{

                        objectValue = getObjectValue(objectField, respValue);

                    }

                    if (objectValue == null) return null;
                    return (String)(objectValue);

                }else{

                    if (respValue == null) return null;
                    return (String)(respValue);
                }
            }
            return null;
        }

        Boolean passesSpec(Object objectInstance, DataPartial specPartial, DataPartial dataPartial, ViewCache resp) {
            if(dataPartial.isWithinIterable() && passesIterableSpec(specPartial, objectInstance, resp)){
                return true;
            }
            if(!dataPartial.isWithinIterable() && passesSpec(specPartial, resp)){
                return true;
            }
            return false;
        }

        Object getObjectMethodValue(ViewCache resp, Object respValue, String objectField){
            MethodInfo activeMethod = getObjectMethod(respValue, objectField);
            String[] parameters = getMethodParameters(objectField);
            ArrayList values = new ArrayList();
            for(int foo = 0; foo < parameters.Length; foo++){
                String parameter = parameters[foo].Trim();
                Object parameterValue = resp.get(parameter);
                values.Add(parameterValue);
            }

            Object activeObjectValue = activeMethod.Invoke(respValue, values.ToArray());
            return activeObjectValue;
        }

        String[] getMethodParameters(String objectField){
            String[] activeMethodAttributes = objectField.Split("\\(");
            String methodParameters = activeMethodAttributes[ONE];
            String activeMethod = methodParameters.Replace("(", "").Replace(")", "");
            String[] parameters = activeMethod.Split(",");
            return parameters;
        }


        MethodInfo getObjectMethod(Object activeObject, String objectField) {
            String[] activeMethodAttributes = objectField.Split("\\(");
            String activeMethodName = activeMethodAttributes[ZERO];
            MethodInfo[] activeObjectMethods = activeObject.GetType().GetMethods();
            MethodInfo activeMethod = null;
            foreach(MethodInfo activeObjectMethod in activeObjectMethods){
                if(activeObjectMethod.Name.Equals(activeMethodName)){
                    activeMethod = activeObjectMethod;
                    break;
                }
            }
            return activeMethod;
        }

        Boolean isObjectMethod(Object respValue, String objectField) {
            String[] activeMethodAttributes = objectField.Split("\\(");
            String activeMethodName = activeMethodAttributes[ZERO];
            MethodInfo[] activeObjectMethods = respValue.GetType().GetMethods();
            foreach(MethodInfo activeMethod in activeObjectMethods){
                if(activeMethod.Name.Equals(activeMethodName))return true;
            }
            return false;
        }

        String getObjectValueForLineComponent(String objectField, Object objectInstance){

            if(objectField.Contains(".")){
                String[] objectFields = objectField.Split("\\.");

                Object activeObject = objectInstance;
                foreach(String activeObjectField in objectFields){
                    activeObject = getObjectValue(activeObjectField, activeObject);
                }

                if(activeObject == null)return "";
                return (String)(activeObject);
            }else {
                if(hasDeclaredField(objectField, objectInstance)) {
                    Object objectValue = getObjectValue(objectField, objectInstance);
                    if (objectValue == null) return null;
                    return (String)(objectValue);
                }else{
                    return (String)(objectInstance);
                }
            }
        }

        Boolean hasDeclaredField(String objectField, Object objectInstance) {
            FieldInfo[] declaredFields = objectInstance.GetType().GetFields();
            foreach(FieldInfo declaredField in declaredFields){
                if(declaredField.Name.Equals(objectField))return true;
            }
            return false;
        }

        Object getObjectValue(String objectField, Object objectInstance){
            FieldInfo fieldObject = objectInstance.GetType().GetField(objectField);
            Object objectValue = fieldObject.GetValue(objectInstance);
            return objectValue;
        }

    }
}