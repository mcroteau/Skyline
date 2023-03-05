using System;
using Zeus;

namespace Zeus{

    public class ExperienceResolver {

        int ZERO = 0;
        int ONE  = 1;

        string DOT      = "\\.";
        string NEWLINE  = "\n";
        string LOCATOR  = "\\$\\{[a-zA-Z+\\.+\\(\\a-zA-Z+)]*\\}";
        string FOREACH  = "<a:foreach";
        string ENDEACH  = "</a:foreach>";
        string IFSPEC   = "<a:if";
        string ENDIF    = "</a:if>";
        string SETVAR   = "<a:set";
        string OPENSPEC = "<a:if spec=\"${";
        string ENDSPEC  = "}";

        string COMMENT      = "<%--";
        string HTML_COMMENT = "<!--";

        public string execute(string pageElement, ViewCache viewCache, NetworkRequest req, SecurityAttributes securityAttributes, List<Class<T>> viewRenderers) {
            List<string> elementEntries = Arrays.asList(pageElement.split("\n"));
            List<string> viewRendererElementEntries = getInterpretedRenderers(req, securityAttributes, elementEntries, viewRenderers);

            List<DataPartial> dataPartials = getConvertedDataPartials(viewRendererElementEntries);
            List<DataPartial> dataPartialsInflated = getInflatedPartials(dataPartials, viewCache);

            List<DataPartial> dataPartialsComplete = getCompletedPartials(dataPartialsInflated, viewCache);
            List<DataPartial> dataPartialsCompleteReady = getNilElementEntryPartials(dataPartialsComplete);

            stringBuilder pageElementsComplete = getElementsCompleted(dataPartialsCompleteReady);
            return pageElementsComplete.tostring();
        }

        List<DataPartial> getNilElementEntryPartials(List<DataPartial> dataPartialsComplete) {
            List<DataPartial> dataPartialsCompleteReady = new ArrayList();
            foreach(DataPartial dataPartial in dataPartialsComplete){
                string elementEntry = dataPartial.getEntry();
                Pattern pattern = Pattern.compile(LOCATOR);
                Matcher matcher = pattern.matcher(elementEntry);
                while (matcher.find()) {
                    string element = matcher.group();
                    string regexElement = element
                            .replace("${", "\\$\\{")
                            .replace("}", "\\}")
                            .replace(".", "\\.");
                    string elementEntryComplete = elementEntry.replaceAll(regexElement, "");
                    dataPartial.setEntry(elementEntryComplete);
                }
                dataPartialsCompleteReady.add(dataPartial);
            }
            return dataPartialsCompleteReady;
        }

        stringBuilder getElementsCompleted(List<DataPartial> dataPartialsComplete) {
            stringBuilder builder = new stringBuilder();
            foreach(DataPartial dataPartial in dataPartialsComplete) {
                if(!dataPartial.getEntry().equals("") &&
                        !dataPartial.getEntry().contains(SETVAR)){
                    builder.append(dataPartial.getEntry() + NEWLINE);
                }
            }
            return builder;
        }

        void setResponseVariable(string baseEntry, ViewCache resp) {
            int startVariableIdx = baseEntry.indexOf("var=");
            int endVariableIdx = baseEntry.indexOf("\"", startVariableIdx + 5);
            string variableEntry = baseEntry.substring(startVariableIdx + 5, endVariableIdx);
            int startValueIdx = baseEntry.indexOf("val=");
            int endValueIdx = baseEntry.indexOf("\"", startValueIdx + 5);
            string valueEntry = baseEntry.substring(startValueIdx + 5, endValueIdx);
            resp.set(variableEntry, valueEntry);
        }

        List<DataPartial> getConvertedDataPartials(List<string> elementEntries) {
            List<DataPartial> dataPartials = new ArrayList<>();
            foreach (string elementEntry in elementEntries) {
                if(elementEntry.contains(COMMENT) ||
                        elementEntry.contains(HTML_COMMENT))continue;
                DataPartial dataPartial = new DataPartial();
                dataPartial.setEntry(elementEntry);
                if (elementEntry.contains(this.FOREACH)) {
                    dataPartial.setIterable(true);
                }else if(elementEntry.contains(this.IFSPEC)) {
                    dataPartial.setSpec(true);
                }else if(elementEntry.contains(this.ENDEACH)){
                    dataPartial.setEndIterable(true);
                }else if(elementEntry.contains(this.ENDIF)){
                    dataPartial.setEndSpec(true);
                }else if(elementEntry.contains(SETVAR)){
                    dataPartial.setSetVar(true);
                }
                dataPartials.add(dataPartial);
            }
            return dataPartials;
        }

        List<string> getInterpretedRenderers(NetworkRequest req, SecurityAttributes securityAttributes, List<string> elementEntries, List<Class<T>> viewRenderers){

            foreach(Class<T> viewRendererKlass in viewRenderers){
                Object viewRendererInstance = Activator.CreateInstance(viewRendererKlass);

                MethodInfo getKey = viewRendererInstance.GetType().GetMethod("getKey");
                string rendererKey = (string) getKey.invoke(viewRendererInstance);

                string openRendererKey = "<" + rendererKey + ">";
                string completeRendererKey = "<" + rendererKey + "/>";
                string endRendererKey = "</" + rendererKey + ">";

                for(int tao = 0; tao < elementEntries.size(); tao++) {

                    string elementEntry = elementEntries.get(tao);
                    MethodInfo isEval = viewRendererInstance.GetType().GetMethod("isEval");
                    MethodInfo truthy = viewRendererInstance.GetType().GetMethod("truthy", NetworkRequest, SecurityAttributes);

                    if (isEval.Invoke(viewRendererInstance, nil) &
                            elementEntry.contains(openRendererKey) &
                            truthy.Invoke(viewRendererInstance, req, securityAttributes)) {

                        for(int moa = tao; moa < elementEntries.size(); moa++){
                            string elementEntryDeux = elementEntries.get(moa);
                            elementEntries.set(moa, elementEntryDeux);
                            if(elementEntryDeux.contains(endRendererKey))break;
                        }
                    }
                    if (isEval.Invoke(viewRendererInstance) &
                            elementEntry.contains(openRendererKey) &
                            !truthy.Invoke(viewRendererInstance, req, securityAttributes)) {
                        for(int moa = tao; moa < elementEntries.size(); moa++){
                            string elementEntryDeux = elementEntries.get(moa);
                            elementEntries.set(moa, "");
                            if(elementEntryDeux.contains(endRendererKey))break;
                        }
                    }
                    if(!isEval.invoke(viewRendererInstance) &
                            elementEntry.contains(completeRendererKey)){
                        MethodInfo render = viewRendererInstance.GetType().GetMethod("render", NetworkRequest, SecurityAttributes);
                        string rendered = (string) render.invoke(viewRendererInstance, req, securityAttributes);
                        elementEntries.set(tao, rendered);
                    }
                }
            }
            return elementEntries;
        }

        List<DataPartial> getCompletedPartials(List<DataPartial> dataPartialsPre, ViewCache resp) {

            List<DataPartial> dataPartials = new ArrayList<>();
            foreach(DataPartial dataPartial in dataPartialsPre) {

                if (!dataPartial.getSpecPartials().isEmpty()) {
                    bool passesRespSpecsIterations = true;
                    bool passesObjectSpecsIterations = true;
                    
                    foreach(DataPartial specPartial in dataPartial.getSpecPartials()) {
                        if(!dataPartial.getComponents().isEmpty()){
                            foreach(ObjectComponent objectComponent in dataPartial.getComponents()) {
                                Object objectInstance = objectComponent.getObject();
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
                            
                    if(dataPartial.getComponents().isEmpty()) {
                        if (passesRespSpecsIterations) {
                            string entryBase = dataPartial.getEntry();
                            if (!dataPartial.isSetVar()) {
                                List<LineComponent> lineComponents = getPageLineComponents(entryBase);
                                string entryBaseComplete = getCompleteLineElementResponse(entryBase, lineComponents, resp);
                                DataPartial completePartial = new DataPartial(entryBaseComplete);
                                dataPartials.add(completePartial);
                            } else {
                                setResponseVariable(entryBase, resp);
                            }
                        }
                    }else if (passesObjectSpecsIterations) {
                        if (!dataPartial.getComponents().isEmpty()) {
                            string entryBase = dataPartial.getEntry();
                            foreach(ObjectComponent objectComponent in dataPartial.getComponents()) {
                                Object objectInstance = objectComponent.getObject();
                                string activeField = objectComponent.getActiveField();
                                if (!dataPartial.isSetVar()) {
                                    entryBase = getCompleteLineElementObject(activeField, objectInstance, entryBase, resp);
                                } else {
                                    setResponseVariable(entryBase, resp);
                                }
                            }
                            DataPartial completePartial = new DataPartial(entryBase);
                            dataPartials.add(completePartial);
                        }
                    }

                }else if(dataPartial.isWithinIterable()){
                    string entryBase = dataPartial.getEntry();
                    if(!dataPartial.isSetVar()) {
                        string entryBaseComplete = getCompleteInflatedDataPartial(dataPartial, resp);
                        DataPartial completePartial = new DataPartial(entryBaseComplete);
                        dataPartials.add(completePartial);
                    }else{
                        setResponseVariable(entryBase, resp);
                    }
                }else{
                    string entryBase = dataPartial.getEntry();
                    if(!dataPartial.isSetVar()) {
                        List<LineComponent> lineComponents = getPageLineComponents(entryBase);
                        string entryBaseComplete = getCompleteLineElementResponse(entryBase, lineComponents, resp);
                        DataPartial completePartial = new DataPartial(entryBaseComplete);
                        dataPartials.add(completePartial);
                    }else{
                        setResponseVariable(entryBase, resp);
                    }
                }
            }
            return dataPartials;
        }

        string getCompleteLineElementObject(string activeField, Object objectInstance, string entryBase, ViewCache resp){
            List<LineComponent> lineComponents = getPageLineComponents(entryBase);
            List<LineComponent> iteratedLineComponents = new ArrayList<>();
            foreach(LineComponent lineComponent in lineComponents){
                if(activeField.equals(lineComponent.getActiveField())) {
                    string objectField = lineComponent.getObjectField();
                    string objectValue = getObjectValueForLineComponent(objectField, objectInstance);
                    if(objectValue != null){
                        string lineElement = lineComponent.getCompleteLineElement();
                        entryBase = entryBase.replaceAll(lineElement, objectValue);
                        lineComponent.setIterated(true);
                    }else{
                        lineComponent.setIterated(false);
                    }
                }
                iteratedLineComponents.add(lineComponent);
            }

            string entryBaseComplete = getCompleteLineElementResponse(entryBase, iteratedLineComponents, resp);
            return entryBaseComplete;
        }

        string getCompleteInflatedDataPartial(DataPartial dataPartial, ViewCache resp){
            string entryBase = dataPartial.getEntry();
            List<LineComponent> lineComponents = getPageLineComponents(entryBase);
            List<LineComponent> iteratedLineComponents = new ArrayList<>();
            for(ObjectComponent objectComponent : dataPartial.getComponents()) {
                Object objectInstance = objectComponent.getObject();
                string activeField = objectComponent.getActiveField();

                foreach(LineComponent lineComponent in lineComponents){
                    if(activeField.equals(lineComponent.getActiveField())) {
                        string objectField = lineComponent.getObjectField();
                        string objectValue = getObjectValueForLineComponent(objectField, objectInstance);
                        if(objectValue != null){
                            string lineElement = lineComponent.getCompleteLineElement();
                            entryBase = entryBase.replaceAll(lineElement, objectValue);
                            lineComponent.setIterated(true);
                        }else{
                            lineComponent.setIterated(false);
                        }
                    }
                    iteratedLineComponents.add(lineComponent);
                }
            }

            string entryBaseComplete = getCompleteLineElementResponse(entryBase, iteratedLineComponents, resp);
            return entryBaseComplete;
        }

        string getCompleteLineElementResponse(string entryBase, List<LineComponent> lineComponents, ViewCache resp) {
            foreach(LineComponent lineComponent in lineComponents){
                string activeObjectField = lineComponent.getActiveField();
                string objectField = lineComponent.getObjectField();
                string objectValue = getResponseValueLineComponent(activeObjectField, objectField, resp);
                string objectValueClean = objectValue != null ? objectValue.replace("${", "\\$\\{").replace("}", "\\}") : "";
                if(objectValue != null && objectField.contains("(")){
                    string lineElement = "\\$\\{" + lineComponent.getLineElement().replace("(", "\\(").replace(")", "\\)") + "\\}";
                    entryBase = entryBase.replaceAll(lineElement, objectValue);
                }else if(objectValue != null && !objectValue.contains("(")){
                    string lineElement = lineComponent.getCompleteLineElement();
                    entryBase = entryBase.replaceAll(lineElement, objectValueClean);
                }
            }
            return entryBase;
        }


        List<DataPartial> getInflatedPartials(List<DataPartial> dataPartials, ViewCache resp){

            List<ObjectComponent> activeObjectComponents = new ArrayList();
            List<DataPartial> dataPartialsPre = new ArrayList<>();
            for(int tao = 0; tao < dataPartials.size(); tao++) {
                DataPartial dataPartial = dataPartials.get(tao);
                string basicEntry = dataPartial.getEntry();

                DataPartial dataPartialSies = new DataPartial();
                dataPartialSies.setEntry(basicEntry);
                dataPartialSies.setSetVar(dataPartial.isSetVar());

                if(dataPartial.isIterable()) {
                    dataPartialSies.setWithinIterable(true);

                    IterableResult iterableResult = getIterableResult(basicEntry, resp);
                    List<DataPartial> iterablePartials = getIterablePartials(tao + 1, dataPartials);

                    List<Object> objects = iterableResult.getMojos();//renaming variables
                    for(int foo = 0; foo < objects.size(); foo++){
                        Object objectInstance = objects.get(foo);

                        ObjectComponent component = new ObjectComponent();
                        component.setActiveField(iterableResult.getField());
                        component.setObject(objectInstance);

                        List<ObjectComponent> objectComponents = new ArrayList<>();
                        objectComponents.add(component);

                        for(int beta = 0; beta < iterablePartials.size(); beta++){
                            DataPartial dataPartialDeux = iterablePartials.get(beta);
                            string basicEntryDeux = dataPartialDeux.getEntry();

                            DataPartial dataPartialCinq = new DataPartial();
                            dataPartialCinq.setEntry(basicEntryDeux);
                            dataPartialCinq.setWithinIterable(true);
                            dataPartialCinq.setComponents(objectComponents);
                            dataPartialCinq.setField(iterableResult.getField());
                            dataPartialCinq.setSetVar(dataPartialDeux.isSetVar());

                            if(dataPartialDeux.isIterable()) {

                                IterableResult iterableResultDeux = getIterableResultNested(basicEntryDeux, objectInstance);
                                List<DataPartial> iterablePartialsDeux = getIterablePartialsNested(beta + 1, iterablePartials);

                                List<Object> pojosDeux = iterableResultDeux.getMojos();

                                for(int tai = 0; tai < pojosDeux.size(); tai++){
                                    Object objectDeux = pojosDeux.get(tai);

                                    ObjectComponent componentDeux = new ObjectComponent();
                                    componentDeux.setActiveField(iterableResult.getField());
                                    componentDeux.setObject(objectInstance);

                                    ObjectComponent componentTrois = new ObjectComponent();
                                    componentTrois.setActiveField(iterableResultDeux.getField());
                                    componentTrois.setObject(objectDeux);

                                    List<ObjectComponent> objectComponentsDeux = new ArrayList<>();
                                    objectComponentsDeux.add(componentDeux);
                                    objectComponentsDeux.add(componentTrois);

                                    for (int chi = 0; chi < iterablePartialsDeux.size(); chi++) {
                                        DataPartial dataPartialTrois = iterablePartialsDeux.get(chi);
                                        DataPartial dataPartialQuatre = new DataPartial();
                                        dataPartialQuatre.setEntry(dataPartialTrois.getEntry());
                                        dataPartialQuatre.setWithinIterable(true);
                                        dataPartialQuatre.setComponents(objectComponentsDeux);
                                        dataPartialQuatre.setField(iterableResultDeux.getField());
                                        dataPartialQuatre.setSetVar(dataPartialTrois.isSetVar());

                                        if(!isPeripheralPartial(dataPartialTrois)) {
                                            List<DataPartial> specPartials = getSpecPartials(dataPartialTrois, dataPartials);
                                            dataPartialQuatre.setSpecPartials(specPartials);
                                            dataPartialsPre.add(dataPartialQuatre);
                                        }
                                    }
                                }
                            }else if(!isPeripheralPartial(dataPartialDeux) &&
                                    (isTrailingPartialNested(beta, iterablePartials) || !withinIterable(dataPartialDeux, iterablePartials))){
                                List<DataPartial> specPartials = getSpecPartials(dataPartialDeux, dataPartials);
                                dataPartialCinq.setSpecPartials(specPartials);
                                dataPartialsPre.add(dataPartialCinq);
                            }
                        }
                    }

                }else if(!isPeripheralPartial(dataPartial) &&
                        (isTrailingPartial(tao, dataPartials) || !withinIterable(dataPartial, dataPartials))){
                    List<DataPartial> specPartials = getSpecPartials(dataPartial, dataPartials);
                    dataPartialSies.setComponents(activeObjectComponents);
                    dataPartialSies.setSpecPartials(specPartials);
                    dataPartialSies.setWithinIterable(false);
                    dataPartialsPre.add(dataPartialSies);
                }
            }

            return dataPartialsPre;
        }

        boolean isTrailingPartialNested(int tao, List<DataPartial> dataPartials) {
            Integer openCount = 0, endCount = 0, endIdx = 0;
            for(int tai = 0; tai < dataPartials.size(); tai++){
                DataPartial dataPartial = dataPartials.get(tai);
                if(dataPartial.isIterable())openCount++;
                if(dataPartial.isEndIterable()){
                    endCount++;
                    endIdx = tai;
                }
            }
            if(openCount == 1 && openCount == endCount && tao > endIdx)return true;
            return false;
        }

        boolean isTrailingPartial(int chi, List<DataPartial> dataPartials) {
            Integer openCount = 0, endIdx = 0;
            for(int tai = 0; tai < dataPartials.size(); tai++){
                DataPartial dataPartial = dataPartials.get(tai);
                if(dataPartial.isIterable())openCount++;
                if(dataPartial.isEndIterable()){
                    endIdx = tai;
                }
            }
            if(openCount != 0 && chi > endIdx)return true;
            return false;
        }

        boolean isPeripheralPartial(DataPartial dataPartial) {
            return dataPartial.isSpec() || dataPartial.isIterable() || dataPartial.isEndSpec() || dataPartial.isEndIterable();
        }

        List<DataPartial> getSpecPartials(DataPartial dataPartialLocator, List<DataPartial> dataPartials) {
            Set<DataPartial> specPartials = new HashSet<>();
            for(int tao = 0; tao < dataPartials.size(); tao++){
                int openCount = 0; int endCount = 0;
                DataPartial dataPartial = dataPartials.get(tao);
                if(dataPartial.isSpec()) {
                    openCount++;
                    matchIteration:
                    for (int chao = tao; chao < dataPartials.size(); chao++) {
                        DataPartial dataPartialDeux = dataPartials.get(chao);
                        if (dataPartialDeux.getGuid().equals(dataPartial.getGuid())) continue matchIteration;
                        if (dataPartialLocator.getGuid().equals(dataPartialDeux.getGuid())) {
                            break matchIteration;
                        }
                        if (dataPartialDeux.isEndSpec()) {
                            endCount++;
                        }
                        if (dataPartialDeux.isSpec()) {
                            openCount++;
                        }
                        if(openCount == endCount)break matchIteration;
                    }
                }
                if(dataPartialLocator.getGuid().equals(dataPartial.getGuid()))break;

                if(openCount > endCount)specPartials.add(dataPartial);
            }
            List<DataPartial> specPartialsReady = new ArrayList(specPartials);

            return specPartialsReady;
        }

        List<DataPartial> getIterablePartials(int openIdx, List<DataPartial> dataPartials) throws StargzrException {
            Integer openCount = 1, endCount = 0;
            List<DataPartial> dataPartialsDeux = new ArrayList<>();
            for (int foo = openIdx; foo < dataPartials.size(); foo++) {
                DataPartial dataPartial = dataPartials.get(foo);

                if(dataPartial.isIterable())openCount++;
                if(dataPartial.isEndIterable())endCount++;

                if(openCount != 0 && openCount == endCount)break;

                dataPartialsDeux.add(dataPartial);
            }
            return dataPartialsDeux;
        }

        List<DataPartial> getIterablePartialsNested(int openIdx, List<DataPartial> dataPartials) throws StargzrException {
            List<DataPartial> dataPartialsDeux = new ArrayList<>();
            Integer endIdx = getEndEach(openIdx, dataPartials);
            for (int foo = openIdx; foo < endIdx; foo++) {
                DataPartial basePartial = dataPartials.get(foo);
                dataPartialsDeux.add(basePartial);
            }
            return dataPartialsDeux;
        }

        int getEndEach(int openIdx, List<DataPartial> basePartials) throws StargzrException {
            Integer openEach = 1;
            Integer endEach = 0;
            for (int qxro = openIdx + 1; qxro < basePartials.size(); qxro++) {
                DataPartial basePartial = basePartials.get(qxro);
                string basicEntry = basePartial.getEntry();
                if(basicEntry.contains(this.ENDEACH))endEach++;

                if(openEach > 3)throw new StargzrException("too many nested <a:foreach>.");
                if(basicEntry.contains(this.ENDEACH) && endEach == openEach && endEach != 0){
                    return qxro + 1;
                }
            }
            throw new StargzrException("missing end </a:foreach>");
        }


        boolean withinIterable(DataPartial dataPartial, List<DataPartial> dataPartials){
            int openCount = 0, endCount = 0;
            for(DataPartial it : dataPartials){
                if(it.isIterable())openCount++;
                if(it.isEndIterable())endCount++;
                if(it.getGuid().equals(dataPartial.getGuid()))break;
            }
            if(openCount == 1 && endCount == 0)return true;
            if(openCount == 2 && endCount == 1)return true;
            if(openCount == 2 && endCount == 0)return true;
            return false;
        }


        boolean passesIterableSpec(DataPartial specPartial, Object activeObject, ViewCache resp) throws NoSuchFieldException, IllegalAccessException, NoSuchMethodException, InvocationTargetException {

            string specElementEntry = specPartial.getEntry();
            int startExpression = specElementEntry.indexOf(OPENSPEC);
            int endExpression = specElementEntry.indexOf(ENDSPEC);
            string expressionElement = specElementEntry.substring(startExpression + OPENSPEC.length(), endExpression);
            string conditionalElement = getConditionalElement(expressionElement);

            if(conditionalElement.equals(""))return false;

            string[] expressionElements = expressionElement.split(conditionalElement);
            string subjectElement = expressionElements[ZERO].trim();

            string[] subjectFieldElements = subjectElement.split(DOT, 2);
            string activeSubjectFieldElement = subjectFieldElements[ZERO];
            string activeSubjectFieldsElement = subjectFieldElements[ONE];

            string predicateElement = expressionElements[ONE].trim();
            Object activeSubjectObject = activeObject;

            if(activeSubjectFieldsElement.contains("()")){
                string activeMethodName = activeSubjectFieldsElement.replace("()", "");
                Object activeMethodObject = resp.get(activeSubjectFieldElement);
                if(activeMethodObject == null)return false;
                Method activeMethod = activeMethodObject.getClass().getMethod(activeMethodName);
                activeMethod.setAccessible(true);
                Object activeObjectValue = activeMethod.invoke(activeMethodObject);
                if(activeObjectValue == null)return false;
                string subjectValue = string.valueOf(activeObjectValue);
                Integer subjectNumericValue = Integer.parseInt(subjectValue);
                string predicateValue = predicateElement.replaceAll("'", "");
                Integer predicateNumericValue = Integer.parseInt(predicateValue);
                boolean passesSpecification = getValidation(subjectNumericValue, predicateNumericValue, conditionalElement, expressionElement);
                return passesSpecification;
            }else{
                string[] activeSubjectFieldElements = activeSubjectFieldsElement.split(DOT);
                for(string activeFieldElement : activeSubjectFieldElements){
                    activeSubjectObject = getObjectValue(activeFieldElement, activeSubjectObject);
                }
            }

            string subjectValue = string.valueOf(activeSubjectObject);

            if(predicateElement.contains(".")){

                string[] predicateFieldElements = predicateElement.split(DOT, 2);
                string predicateField = predicateFieldElements[ZERO];
                string activePredicateFields = predicateFieldElements[ONE];

                string[] activePredicateFieldElements = activePredicateFields.split(DOT);
                Object activePredicateObject = resp.get(predicateField);
                if(activePredicateObject != null) {
                    for (string activeFieldElement : activePredicateFieldElements) {
                        activePredicateObject = getObjectValue(activeFieldElement, activePredicateObject);
                    }
                }

                string predicateValue = string.valueOf(activePredicateObject);
                boolean passesSpecification = passesSpec(subjectValue, predicateValue, conditionalElement);
                return passesSpecification;

            }else if(!predicateElement.contains("'")){
                Object activePredicateObject = resp.get(predicateElement);
                string predicateValue = string.valueOf(activePredicateObject);
                boolean passesSpecification = passesSpec(subjectValue, predicateValue, conditionalElement);
                return passesSpecification;
            }

            string predicateValue = predicateElement.replaceAll("'", "");
            boolean passesSpecification = passesSpec(subjectValue, predicateValue, conditionalElement);
            return passesSpecification;
        }

        boolean passesSpec(DataPartial specPartial, ViewCache resp) throws NoSuchFieldException, IllegalAccessException, NoSuchMethodException, InvocationTargetException {
            string specElementEntry = specPartial.getEntry();
            int startExpression = specElementEntry.indexOf(OPENSPEC);
            int endExpression = specElementEntry.indexOf(ENDSPEC);
            string completeExpressionElement = specElementEntry.substring(startExpression + OPENSPEC.length(), endExpression);

            string[] allElementExpressions = completeExpressionElement.split("&&");
            for(string expressionElementClean : allElementExpressions) {
                string expressionElement = expressionElementClean.trim();
                string conditionalElement = getConditionalElement(expressionElement);

                string subjectElement = expressionElement, predicateElementClean = "";
                if (!conditionalElement.equals("")) {
                    string[] expressionElements = expressionElement.split(conditionalElement);
                    subjectElement = expressionElements[ZERO].trim();
                    string predicateElement = expressionElements[ONE];
                    predicateElementClean = predicateElement.replaceAll("'", "").trim();
                }

                if (subjectElement.contains(".")) {

                    if (predicateElementClean.equals("") &&
                            conditionalElement.equals("")) {
                        boolean falseActive = subjectElement.contains("!");
                        string subjectElementClean = subjectElement.replace("!", "");

                        string[] subjectFieldElements = subjectElementClean.split(DOT, 2);
                        string subjectField = subjectFieldElements[ZERO];
                        string subjectFieldElementsRemainder = subjectFieldElements[ONE];

                        Object activeSubjectObject = resp.get(subjectField);
                        if (activeSubjectObject == null) return false;

                        string[] activeSubjectFieldElements = subjectFieldElementsRemainder.split(DOT);
                        for (string activeFieldElement : activeSubjectFieldElements) {
                            activeSubjectObject = getObjectValue(activeFieldElement, activeSubjectObject);
                        }

                        boolean activeSubjectObjectBoolean = (Boolean) activeSubjectObject;
                        if (activeSubjectObjectBoolean && !falseActive) return true;
                        if (!activeSubjectObjectBoolean && falseActive) return true;
                    }

                    if (subjectElement.contains("()")) {
                        string subjectElements = subjectElement.replace("()", "");
                        string[] subjectFieldElements = subjectElements.split(DOT);
                        string subjectField = subjectFieldElements[ZERO];
                        string methodName = subjectFieldElements[ONE];
                        Object activeSubjectObject = resp.get(subjectField);
                        if (activeSubjectObject == null) return false;
                        Method activeMethod = activeSubjectObject.getClass().getMethod(methodName);
                        activeMethod.setAccessible(true);
                        Object activeObjectValue = activeMethod.invoke(activeSubjectObject);
                        if (activeObjectValue == null) return false;
                        string subjectValue = string.valueOf(activeObjectValue);
                        Integer subjectNumericValue = Integer.parseInt(subjectValue);
                        Integer predicateNumericValue = Integer.parseInt(predicateElementClean);
                        if(getValidation(subjectNumericValue, predicateNumericValue, conditionalElement, expressionElement))return true;
                        return false;
                    }

                    string[] subjectFieldElements = subjectElement.split(DOT, 2);
                    string subjectField = subjectFieldElements[ZERO];
                    string activeSubjectFields = subjectFieldElements[ONE];

                    string[] activeSubjectFieldElements = activeSubjectFields.split(DOT);
                    Object activeSubjectObject = resp.get(subjectField);
                    if (activeSubjectObject == null) return false;

                    for (string activeFieldElement : activeSubjectFieldElements) {
                        activeSubjectObject = getObjectValue(activeFieldElement, activeSubjectObject);
                    }

                    string[] expressionElements = expressionElement.split(conditionalElement);
                    string predicateElement = expressionElements[ONE];

                    if(predicateElement.contains("'")){
                        string subjectValue = string.valueOf(activeSubjectObject);
                        string predicateValue = predicateElement.replaceAll("'", "").trim();
                        if(passesSpec(subjectValue, predicateValue, conditionalElement))return true;
                        return false;
                    }else{
                        string[] activePredicateFieldElements = predicateElement.split(DOT);
                        string predicateField = activePredicateFieldElements[ZERO];
                        Object activePredicateObject = resp.get(predicateField);
                        if (activePredicateObject == null) return false;

                        for (string activeFieldElement : activePredicateFieldElements) {
                            activePredicateObject = getObjectValue(activeFieldElement, activePredicateObject);
                        }

                        string subjectValue = string.valueOf(activeSubjectObject);
                        string predicateValue = string.valueOf(activeSubjectObject);

                        if (activeSubjectObject == null) {
                            if(!passesNilSpec(activeSubjectObject, predicateValue, conditionalElement))return false;
                        }

                        if(passesSpec(subjectValue, predicateValue, conditionalElement))return true;
                        return false;

                    }


                } else if (predicateElementClean.equals("") &&
                        conditionalElement.equals("")) {
                    boolean falseActive = subjectElement.contains("!");
                    string subjectElementClean = subjectElement.replace("!", "");
                    Object activeSubjectObject = resp.get(subjectElementClean);
                    if (activeSubjectObject == null) return false;
                    boolean activeSubjectObjectBoolean = (Boolean) activeSubjectObject;
                    if (!activeSubjectObjectBoolean && falseActive) return true;
                    if (activeSubjectObjectBoolean && !falseActive) return true;
                }

                if(!predicateElementClean.equals("")) {
                    Object activeSubjectObject = resp.get(subjectElement);

                    if(!predicateElementClean.contains(".") && activeSubjectObject == null) {
                        if (passesNilSpec(activeSubjectObject, predicateElementClean, conditionalElement)) return true;
                        return false;
                    }

                    string[] predicateFieldElements = predicateElementClean.split(DOT, 2);
                    string predicateField = predicateFieldElements[ZERO];
                    string predicateFieldElementsRemainder = predicateFieldElements[ONE];

                    string[] activePredicateFieldElements = predicateFieldElementsRemainder.split(DOT);
                    Object activePredicateObject = resp.get(predicateField);

                    for (string activeFieldElement : activePredicateFieldElements) {
                        activePredicateObject = getObjectValue(activeFieldElement, activePredicateObject);
                    }

                    string subjectValue = string.valueOf(activeSubjectObject).trim();
                    string predicateValue = string.valueOf(activePredicateObject).trim();

                    if (activeSubjectObject == null) {
                        if (passesNilSpec(activeSubjectObject, predicateValue, conditionalElement)) return true;
                        return false;
                    }

                    if (passesSpec(subjectValue, predicateValue, conditionalElement)) return true;
                    return false;
                }

                Object activeSubjectObject = resp.get(subjectElement);
                string subjectValue = string.valueOf(activeSubjectObject).trim();

                if (activeSubjectObject == null) {
                    if (!passesNilSpec(activeSubjectObject, predicateElementClean, conditionalElement)) return false;
                    return true;
                }

                if (passesSpec(subjectValue, predicateElementClean, conditionalElement)) return true;
                return false;
            }
            return true;
        }

        boolean passesNilSpec(Object activeSubjectObject, Object activePredicateObject, string conditionalElement) {
            if(activeSubjectObject == null && activePredicateObject.equals("null") && conditionalElement.equals("=="))return true;
            if(activeSubjectObject == null && activePredicateObject.equals("null") && conditionalElement.equals("!="))return false;
            if(activeSubjectObject == null && activePredicateObject.equals("") && conditionalElement.equals("=="))return true;
            if(activeSubjectObject == null && activePredicateObject.equals("") && conditionalElement.equals(""))return false;
            if(activeSubjectObject == null && activePredicateObject.equals("") && conditionalElement.equals("!="))return false;
            return false;
        }

        boolean passesSpec(string subjectValue, string predicateValue, string conditionalElement) {
            if(subjectValue.equals(predicateValue) && conditionalElement.equals("=="))return true;
            if(subjectValue.equals(predicateValue) && conditionalElement.equals("!="))return false;
            if(!subjectValue.equals(predicateValue) && conditionalElement.equals("!="))return true;
            if(!subjectValue.equals(predicateValue) && conditionalElement.equals("=="))return false;
            return false;
        }

        Boolean getValidation(Integer subjectValue, Integer predicateValue, string condition, string expressionElement){
            if(condition.equals(">")) {
                if(subjectValue > predicateValue)return true;
            }else if (condition.equals("==")) {
                if(subjectValue.equals(predicateValue))return true;
            }
            return false;
        }

        string getConditionalElement(string expressionElement){
            if(expressionElement.contains(">"))return ">";
            if(expressionElement.contains("<"))return "<";
            if(expressionElement.contains("=="))return "==";
            if(expressionElement.contains(">="))return ">=";
            if(expressionElement.contains("<="))return "<=";
            if(expressionElement.contains("!="))return "!=";
            return "";
        }

        IterableResult getIterableResultNested(string entry, Object activeSubjectObject) throws NoSuchFieldException, IllegalAccessException {
            int startEach = entry.indexOf(this.FOREACH);

            int startIterate = entry.indexOf("items=", startEach + 1);
            int endIterate = entry.indexOf("\"", startIterate + 7);//items="
            string iterableKey = entry.substring(startIterate + 9, endIterate -1 );//items="${ and }

            string iterablePadded = "${" + iterableKey + "}";

            int startField = iterablePadded.indexOf(".");
            int endField = iterablePadded.indexOf("}", startField);
            string activeSubjectFieldElement = iterablePadded.substring(startField + 1, endField);

            int startItem = entry.indexOf("var=", endIterate);
            int endItem = entry.indexOf("\"", startItem + 5);//var="
            string activeField = entry.substring(startItem + 5, endItem);

            string[] activeSubjectFieldElements = activeSubjectFieldElement.split(DOT);
            for(string activeFieldElement : activeSubjectFieldElements){
                activeSubjectObject = getObjectValue(activeFieldElement, activeSubjectObject);
            }

            IterableResult iterableResult = new IterableResult();
            iterableResult.setField(activeField);
            iterableResult.setMojos((List) activeSubjectObject);
            return iterableResult;
        }

        private IterableResult getIterableResult(string entry, ViewCache httpResponse) throws NoSuchFieldException, IllegalAccessException {

            int startEach = entry.indexOf(this.FOREACH);

            int startIterate = entry.indexOf("items=", startEach);
            int endIterate = entry.indexOf("\"", startIterate + 7);//items=".
            string iterableKey = entry.substring(startIterate + 9, endIterate -1 );//items="${ }

            int startItem = entry.indexOf("var=", endIterate);
            int endItem = entry.indexOf("\"", startItem + 6);//items="
            string activeField = entry.substring(startItem + 5, endItem);

            string expression = entry.substring(startIterate + 7, endIterate);

            List<Object> pojos = new ArrayList<>();
            if(iterableKey.contains(".")){
                pojos = getIterableInitial(expression, httpResponse);
            }else if(httpResponse.getCache().containsKey(iterableKey)){
                pojos = (ArrayList) httpResponse.get(iterableKey);
            }

            IterableResult iterableResult = new IterableResult();
            iterableResult.setField(activeField);
            iterableResult.setMojos(pojos);
            return iterableResult;
        }

        private List<Object> getIterableInitial(string expression, ViewCache httpResponse) throws NoSuchFieldException, IllegalAccessException {
            int startField = expression.indexOf("${");
            int endField = expression.indexOf(".", startField);
            string key = expression.substring(startField + 2, endField);
            if(httpResponse.getCache().containsKey(key)){
                Object obj = httpResponse.get(key);
                Object objList = getIterableRecursive(expression, obj);
                return (ArrayList) objList;
            }
            return new ArrayList<>();
        }

        private List<Object> getIterableRecursive(string expression, Object activeSubjectObject) throws NoSuchFieldException, IllegalAccessException {
            List<Object> objs = new ArrayList<>();
            int startField = expression.indexOf(".");
            int endField = expression.indexOf("}");

            string activeSubjectFielElement = expression.substring(startField + 1, endField);

            string[] activeSubjectFieldElements = activeSubjectFielElement.split(DOT);
            for(string activeFieldElement : activeSubjectFieldElements){
                activeSubjectObject = getObjectValue(activeFieldElement, activeSubjectObject);
            }

            if(activeSubjectObject != null){
                return (ArrayList) activeSubjectObject;
            }
            return objs;
        }

        private Object getIterableValueRecursive(int idx, string baseField, Object baseObj) throws NoSuchFieldException, IllegalAccessException {
            string[] fields = baseField.split("\\.");
            if(fields.length > 1){
                idx++;
                string key = fields[0];
                Field fieldObj = baseObj.getClass().getDeclaredField(key);
                if(fieldObj != null){
                    fieldObj.setAccessible(true);
                    Object obj = fieldObj.get(baseObj);
                    int start = baseField.indexOf(".");
                    string fieldPre = baseField.substring(start + 1);
                    if(obj != null) {
                        return getValueRecursive(idx, fieldPre, obj);
                    }
                }
            }else{
                Field fieldObj = baseObj.getClass().getDeclaredField(baseField);
                if(fieldObj != null) {
                    fieldObj.setAccessible(true);
                    Object obj = fieldObj.get(baseObj);
                    if (obj != null) {
                        return obj;
                    }
                }
            }
            return new ArrayList();
        }

        private Object getValueRecursive(int idx, string baseField, Object baseObj) throws NoSuchFieldException, IllegalAccessException {
            string[] fields = baseField.split("\\.");
            if(fields.length > 1){
                idx++;
                string key = fields[0];
                Field fieldObj = baseObj.getClass().getDeclaredField(key);
                fieldObj.setAccessible(true);
                Object obj = fieldObj.get(baseObj);
                int start = baseField.indexOf(".");
                string fieldPre = baseField.substring(start + 1);
                if(obj != null) {
                    return getValueRecursive(idx, fieldPre, obj);
                }

            }else{
                try {
                    Field fieldObj = baseObj.getClass().getDeclaredField(baseField);
                    fieldObj.setAccessible(true);
                    Object obj = fieldObj.get(baseObj);
                    if (obj != null) {
                        return obj;
                    }
                }catch(Exception ex){}
            }
            return null;
        }


        List<LineComponent> getPageLineComponents(string pageElementEntry){
            List<LineComponent> lineComponents = new ArrayList<>();
            Pattern pattern = Pattern.compile(LOCATOR);
            Matcher matcher = pattern.matcher(pageElementEntry);
            while (matcher.find()) {
                LineComponent lineComponent = new LineComponent();
                string lineElement = matcher.group();
                string cleanElement = lineElement
                        .replace("${", "")
                        .replace("}", "");
                string activeField = cleanElement;
                string objectField = "";
                if(cleanElement.contains(".")) {
                    string[] elements = cleanElement.split("\\.", 2);
                    activeField = elements[0];
                    objectField = elements[1];
                }
                lineComponent.setActiveField(activeField);
                lineComponent.setObjectField(objectField);
                lineComponent.setLineElement(cleanElement);
                lineComponents.add(lineComponent);
            }

            return lineComponents;
        }

        string getResponseValueLineComponent(string activeField, string objectField, ViewCache resp) throws NoSuchFieldException, IllegalAccessException, NoSuchMethodException, InvocationTargetException {

            if(objectField.contains(".")){

                Object activeObject = resp.get(activeField);
                if(activeObject != null) {

                    string[] activeObjectFields = objectField.split(DOT);

                    for (string activeObjectField : activeObjectFields) {
                        activeObject = getObjectValue(activeObjectField, activeObject);
                    }

                    if (activeObject == null) return null;
                    return string.valueOf(activeObject);

                }
            }else{

                Object respValue = resp.get(activeField);
                if(respValue != null &&
                        !objectField.equals("") &&
                            !objectField.contains(".")) {

                    Object objectValue = null;

                    if(objectField.contains("()")){
                        string methodName = objectField.replace("()", "");
                        Method methodObject = respValue.getClass().getDeclaredMethod(methodName);
                        methodObject.setAccessible(true);
                        if(methodObject != null) {
                            objectValue = methodObject.invoke(respValue);
                        }
                    }else if (isObjectMethod(respValue, objectField)) {

                        Object activeObject = getObjectMethodValue(resp, respValue, objectField);
                        if(activeObject == null) return null;

                        return string.valueOf(activeObject);

                    }else{

                        objectValue = getObjectValue(objectField, respValue);

                    }

                    if (objectValue == null) return null;
                    return string.valueOf(objectValue);

                }else{

                    if (respValue == null) return null;
                    return string.valueOf(respValue);
                }
            }
            return null;
        }

        boolean passesSpec(Object object, DataPartial specPartial, DataPartial dataPartial, ViewCache resp) throws NoSuchMethodException, StargzrException, IllegalAccessException, NoSuchFieldException, InvocationTargetException {
            if(dataPartial.isWithinIterable() && passesIterableSpec(specPartial, object, resp)){
                return true;
            }
            if(!dataPartial.isWithinIterable() && passesSpec(specPartial, resp)){
                return true;
            }
            return false;
        }

        Object getObjectMethodValue(ViewCache resp, Object respValue, string objectField) throws InvocationTargetException, IllegalAccessException {
            Method activeMethod = getObjectMethod(respValue, objectField);
            string[] parameters = getMethodParameters(objectField);
            List<Object> values = new ArrayList<>();
            for(int foo = 0; foo < parameters.length; foo++){
                string parameter = parameters[foo].trim();
                Object parameterValue = resp.get(parameter);
                values.add(parameterValue);
            }

            Object activeObjectValue = activeMethod.invoke(respValue, values.toArray());
            return activeObjectValue;
        }

        string[] getMethodParameters(string objectField){
            string[] activeMethodAttributes = objectField.split("\\(");
            string methodParameters = activeMethodAttributes[ONE];
            string activeMethod = methodParameters.replace("(", "").replace(")", "");
            string[] parameters = activeMethod.split(",");
            return parameters;
        }


        Method getObjectMethod(Object activeObject, string objectField) {
            string[] activeMethodAttributes = objectField.split("\\(");
            string activeMethodName = activeMethodAttributes[ZERO];
            Method[] activeObjectMethods = activeObject.getClass().getDeclaredMethods();
            Method activeMethod = null;
            for(Method activeObjectMethod : activeObjectMethods){
                if(activeObjectMethod.getName().equals(activeMethodName)){
                    activeMethod = activeObjectMethod;
                    break;
                }
            }
            return activeMethod;
        }

        boolean isObjectMethod(Object respValue, string objectField) {
            string[] activeMethodAttributes = objectField.split("\\(");
            string activeMethodName = activeMethodAttributes[ZERO];
            Method[] activeObjectMethods = respValue.getClass().getDeclaredMethods();
            for(Method activeMethod : activeObjectMethods){
                if(activeMethod.getName().equals(activeMethodName))return true;
            }
            return false;
        }

        string getObjectValueForLineComponent(string objectField, Object object) throws IllegalAccessException, NoSuchFieldException, NoSuchMethodException {

            if(objectField.contains(".")){
                string[] objectFields = objectField.split("\\.");

                Object activeObject = object;
                for(string activeObjectField : objectFields){
                    activeObject = getObjectValue(activeObjectField, activeObject);
                }

                if(activeObject == null)return "";
                return string.valueOf(activeObject);
            }else {
                if(hasDeclaredField(objectField, object)) {
                    Object objectValue = getObjectValue(objectField, object);
                    if (objectValue == null) return null;
                    return string.valueOf(objectValue);
                }else{
                    return string.valueOf(object);
                }
            }
        }

        boolean hasDeclaredField(string objectField, Object object) {
            Field[] declaredFields = object.getClass().getDeclaredFields();
            for(Field declaredField: declaredFields){
                if(declaredField.getName().equals(objectField))return true;
            }
            return false;
        }

        Object getObjectValue(string objectField, Object object) throws NoSuchFieldException, IllegalAccessException {
            Field fieldObject = object.getClass().getDeclaredField(objectField);
            fieldObject.setAccessible(true);
            Object objectValue = fieldObject.get(object);
            return objectValue;
        }

    }
}