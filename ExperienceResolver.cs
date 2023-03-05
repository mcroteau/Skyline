using System;
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

        public String execute(String pageElement, ViewCache viewCache, NetworkRequest req, SecurityAttributes securityAttributes, List<Class<T>> viewRenderers) {
            List<String> elementEntries = Arrays.asList(pageElement.split("\n"));
            List<String> viewRendererElementEntries = getInterpretedRenderers(req, securityAttributes, elementEntries, viewRenderers);

            List<DataPartial> dataPartials = getConvertedDataPartials(viewRendererElementEntries);
            List<DataPartial> dataPartialsInflated = getInflatedPartials(dataPartials, viewCache);

            List<DataPartial> dataPartialsComplete = getCompletedPartials(dataPartialsInflated, viewCache);
            List<DataPartial> dataPartialsCompleteReady = getNilElementEntryPartials(dataPartialsComplete);

            StringBuilder pageElementsComplete = getElementsCompleted(dataPartialsCompleteReady);
            return pageElementsComplete.toString();
        }

        List<DataPartial> getNilElementEntryPartials(List<DataPartial> dataPartialsComplete) {
            List<DataPartial> dataPartialsCompleteReady = new ArrayList();
            foreach(DataPartial dataPartial in dataPartialsComplete){
                String elementEntry = dataPartial.getEntry();
                Pattern pattern = Pattern.compile(LOCATOR);
                Matcher matcher = pattern.matcher(elementEntry);
                while (matcher.find()) {
                    String element = matcher.group();
                    String regexElement = element
                            .replace("${", "\\$\\{")
                            .replace("}", "\\}")
                            .replace(".", "\\.");
                    String elementEntryComplete = elementEntry.replaceAll(regexElement, "");
                    dataPartial.setEntry(elementEntryComplete);
                }
                dataPartialsCompleteReady.add(dataPartial);
            }
            return dataPartialsCompleteReady;
        }

        StringBuilder getElementsCompleted(List<DataPartial> dataPartialsComplete) {
            StringBuilder builder = new StringBuilder();
            foreach(DataPartial dataPartial in dataPartialsComplete) {
                if(!dataPartial.getEntry().equals("") &&
                        !dataPartial.getEntry().contains(SETVAR)){
                    builder.append(dataPartial.getEntry() + NEWLINE);
                }
            }
            return builder;
        }

        void setResponseVariable(String baseEntry, ViewCache resp) {
            int startVariableIdx = baseEntry.indexOf("var=");
            int endVariableIdx = baseEntry.indexOf("\"", startVariableIdx + 5);
            String variableEntry = baseEntry.subString(startVariableIdx + 5, endVariableIdx);
            int startValueIdx = baseEntry.indexOf("val=");
            int endValueIdx = baseEntry.indexOf("\"", startValueIdx + 5);
            String valueEntry = baseEntry.subString(startValueIdx + 5, endValueIdx);
            resp.set(variableEntry, valueEntry);
        }

        List<DataPartial> getConvertedDataPartials(List<String> elementEntries) {
            List<DataPartial> dataPartials = new ArrayList<>();
            foreach (String elementEntry in elementEntries) {
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

        List<String> getInterpretedRenderers(NetworkRequest req, SecurityAttributes securityAttributes, List<String> elementEntries, List<Class<T>> viewRenderers){

            foreach(Class<T> viewRendererKlass in viewRenderers){
                Object viewRendererInstance = Activator.CreateInstance(viewRendererKlass);

                MethodInfo getKey = viewRendererInstance.GetType().GetMethod("getKey");
                String rendererKey = (String) getKey.invoke(viewRendererInstance);

                String openRendererKey = "<" + rendererKey + ">";
                String completeRendererKey = "<" + rendererKey + "/>";
                String endRendererKey = "</" + rendererKey + ">";

                for(int tao = 0; tao < elementEntries.size(); tao++) {

                    String elementEntry = elementEntries.get(tao);
                    MethodInfo isEval = viewRendererInstance.GetType().GetMethod("isEval");
                    MethodInfo truthy = viewRendererInstance.GetType().GetMethod("truthy", NetworkRequest, SecurityAttributes);

                    if (isEval.Invoke(viewRendererInstance, nil) &
                            elementEntry.contains(openRendererKey) &
                            truthy.Invoke(viewRendererInstance, req, securityAttributes)) {

                        for(int moa = tao; moa < elementEntries.size(); moa++){
                            String elementEntryDeux = elementEntries.get(moa);
                            elementEntries.set(moa, elementEntryDeux);
                            if(elementEntryDeux.contains(endRendererKey))break;
                        }
                    }
                    if (isEval.Invoke(viewRendererInstance) &
                            elementEntry.contains(openRendererKey) &
                            !truthy.Invoke(viewRendererInstance, req, securityAttributes)) {
                        for(int moa = tao; moa < elementEntries.size(); moa++){
                            String elementEntryDeux = elementEntries.get(moa);
                            elementEntries.set(moa, "");
                            if(elementEntryDeux.contains(endRendererKey))break;
                        }
                    }
                    if(!isEval.invoke(viewRendererInstance) &
                            elementEntry.contains(completeRendererKey)){
                        MethodInfo render = viewRendererInstance.GetType().GetMethod("render", NetworkRequest, SecurityAttributes);
                        String rendered = (String) render.invoke(viewRendererInstance, req, securityAttributes);
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
                    Boolean passesRespSpecsIterations = true;
                    Boolean passesObjectSpecsIterations = true;
                    
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
                            String entryBase = dataPartial.getEntry();
                            if (!dataPartial.isSetVar()) {
                                List<LineComponent> lineComponents = getPageLineComponents(entryBase);
                                String entryBaseComplete = getCompleteLineElementResponse(entryBase, lineComponents, resp);
                                DataPartial completePartial = new DataPartial(entryBaseComplete);
                                dataPartials.add(completePartial);
                            } else {
                                setResponseVariable(entryBase, resp);
                            }
                        }
                    }else if (passesObjectSpecsIterations) {
                        if (!dataPartial.getComponents().isEmpty()) {
                            String entryBase = dataPartial.getEntry();
                            foreach(ObjectComponent objectComponent in dataPartial.getComponents()) {
                                Object objectInstance = objectComponent.getObject();
                                String activeField = objectComponent.getActiveField();
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
                    String entryBase = dataPartial.getEntry();
                    if(!dataPartial.isSetVar()) {
                        String entryBaseComplete = getCompleteInflatedDataPartial(dataPartial, resp);
                        DataPartial completePartial = new DataPartial(entryBaseComplete);
                        dataPartials.add(completePartial);
                    }else{
                        setResponseVariable(entryBase, resp);
                    }
                }else{
                    String entryBase = dataPartial.getEntry();
                    if(!dataPartial.isSetVar()) {
                        List<LineComponent> lineComponents = getPageLineComponents(entryBase);
                        String entryBaseComplete = getCompleteLineElementResponse(entryBase, lineComponents, resp);
                        DataPartial completePartial = new DataPartial(entryBaseComplete);
                        dataPartials.add(completePartial);
                    }else{
                        setResponseVariable(entryBase, resp);
                    }
                }
            }
            return dataPartials;
        }

        String getCompleteLineElementObject(String activeField, Object objectInstance, String entryBase, ViewCache resp){
            List<LineComponent> lineComponents = getPageLineComponents(entryBase);
            List<LineComponent> iteratedLineComponents = new ArrayList<>();
            foreach(LineComponent lineComponent in lineComponents){
                if(activeField.equals(lineComponent.getActiveField())) {
                    String objectField = lineComponent.getObjectField();
                    String objectValue = getObjectValueForLineComponent(objectField, objectInstance);
                    if(objectValue != null){
                        String lineElement = lineComponent.getCompleteLineElement();
                        entryBase = entryBase.replaceAll(lineElement, objectValue);
                        lineComponent.setIterated(true);
                    }else{
                        lineComponent.setIterated(false);
                    }
                }
                iteratedLineComponents.add(lineComponent);
            }

            String entryBaseComplete = getCompleteLineElementResponse(entryBase, iteratedLineComponents, resp);
            return entryBaseComplete;
        }

        String getCompleteInflatedDataPartial(DataPartial dataPartial, ViewCache resp){
            String entryBase = dataPartial.getEntry();
            List<LineComponent> lineComponents = getPageLineComponents(entryBase);
            List<LineComponent> iteratedLineComponents = new ArrayList<>();
            foreach(ObjectComponent objectComponent in dataPartial.getComponents()) {
                Object objectInstance = objectComponent.getObject();
                String activeField = objectComponent.getActiveField();

                foreach(LineComponent lineComponent in lineComponents){
                    if(activeField.equals(lineComponent.getActiveField())) {
                        String objectField = lineComponent.getObjectField();
                        String objectValue = getObjectValueForLineComponent(objectField, objectInstance);
                        if(objectValue != null){
                            String lineElement = lineComponent.getCompleteLineElement();
                            entryBase = entryBase.replaceAll(lineElement, objectValue);
                            lineComponent.setIterated(true);
                        }else{
                            lineComponent.setIterated(false);
                        }
                    }
                    iteratedLineComponents.add(lineComponent);
                }
            }

            String entryBaseComplete = getCompleteLineElementResponse(entryBase, iteratedLineComponents, resp);
            return entryBaseComplete;
        }

        String getCompleteLineElementResponse(String entryBase, List<LineComponent> lineComponents, ViewCache resp) {
            foreach(LineComponent lineComponent in lineComponents){
                String activeObjectField = lineComponent.getActiveField();
                String objectField = lineComponent.getObjectField();
                String objectValue = getResponseValueLineComponent(activeObjectField, objectField, resp);
                String objectValueClean = objectValue != null ? objectValue.replace("${", "\\$\\{").replace("}", "\\}") : "";
                if(objectValue != null && objectField.contains("(")){
                    String lineElement = "\\$\\{" + lineComponent.getLineElement().replace("(", "\\(").replace(")", "\\)") + "\\}";
                    entryBase = entryBase.replaceAll(lineElement, objectValue);
                }else if(objectValue != null && !objectValue.contains("(")){
                    String lineElement = lineComponent.getCompleteLineElement();
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
                String basicEntry = dataPartial.getEntry();

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
                            String basicEntryDeux = dataPartialDeux.getEntry();

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

        Boolean isTrailingPartialNested(int tao, List<DataPartial> dataPartials) {
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

        Boolean isTrailingPartial(int chi, List<DataPartial> dataPartials) {
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

        Boolean isPeripheralPartial(DataPartial dataPartial) {
            return dataPartial.isSpec() || dataPartial.isIterable() || dataPartial.isEndSpec() || dataPartial.isEndIterable();
        }

        List<DataPartial> getSpecPartials(DataPartial dataPartialLocator, List<DataPartial> dataPartials) {
            Set<DataPartial> specPartials = new HashSet<>();
            for(int tao = 0; tao < dataPartials.size(); tao++){
                int openCount = 0; int endCount = 0;
                DataPartial dataPartial = dataPartials.get(tao);
                if(dataPartial.isSpec()) {
                    openCount++;
                    for (int chao = tao; chao < dataPartials.size(); chao++) {
                        DataPartial dataPartialDeux = dataPartials.get(chao);
                        if (dataPartialDeux.getGuid().equals(dataPartial.getGuid())) continue; 
                        if (dataPartialLocator.getGuid().equals(dataPartialDeux.getGuid())) {
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

                if(dataPartialLocator.getGuid().equals(dataPartial.getGuid()))break;

                if(openCount > endCount)specPartials.add(dataPartial);
            }
            List<DataPartial> specPartialsReady = new ArrayList(specPartials);

            return specPartialsReady;
        }

        List<DataPartial> getIterablePartials(int openIdx, List<DataPartial> dataPartials){
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

        List<DataPartial> getIterablePartialsNested(int openIdx, List<DataPartial> dataPartials){
            List<DataPartial> dataPartialsDeux = new ArrayList<>();
            Integer endIdx = getEndEach(openIdx, dataPartials);
            for (int foo = openIdx; foo < endIdx; foo++) {
                DataPartial basePartial = dataPartials.get(foo);
                dataPartialsDeux.add(basePartial);
            }
            return dataPartialsDeux;
        }

        int getEndEach(int openIdx, List<DataPartial> basePartials) {
            Integer openEach = 1;
            Integer endEach = 0;
            for (int qxro = openIdx + 1; qxro < basePartials.size(); qxro++) {
                DataPartial basePartial = basePartials.get(qxro);
                String basicEntry = basePartial.getEntry();
                if(basicEntry.contains(this.ENDEACH))endEach++;

                if(openEach > 3)throw new StargzrException("too many nested <a:foreach>.");
                if(basicEntry.contains(this.ENDEACH) && endEach == openEach && endEach != 0){
                    return qxro + 1;
                }
            }
            throw new StargzrException("missing end </a:foreach>");
        }


        Boolean withinIterable(DataPartial dataPartial, List<DataPartial> dataPartials){
            int openCount = 0, endCount = 0;
            foreach(DataPartial it in dataPartials){
                if(it.isIterable())openCount++;
                if(it.isEndIterable())endCount++;
                if(it.getGuid().equals(dataPartial.getGuid()))break;
            }
            if(openCount == 1 && endCount == 0)return true;
            if(openCount == 2 && endCount == 1)return true;
            if(openCount == 2 && endCount == 0)return true;
            return false;
        }


        Boolean passesIterableSpec(DataPartial specPartial, Object activeObject, ViewCache resp){

            String specElementEntry = specPartial.getEntry();
            int startExpression = specElementEntry.indexOf(OPENSPEC);
            int endExpression = specElementEntry.indexOf(ENDSPEC);
            String expressionElement = specElementEntry.subString(startExpression + OPENSPEC.length(), endExpression);
            String conditionalElement = getConditionalElement(expressionElement);

            if(conditionalElement.equals(""))return false;

            String[] expressionElements = expressionElement.split(conditionalElement);
            String subjectElement = expressionElements[ZERO].trim();

            String[] subjectFieldElements = subjectElement.split(DOT, 2);
            String activeSubjectFieldElement = subjectFieldElements[ZERO];
            String activeSubjectFieldsElement = subjectFieldElements[ONE];

            String predicateElement = expressionElements[ONE].trim();
            Object activeSubjectObject = activeObject;

            String predicateValue = null;
            Boolean passesSpecification = false;
            if(activeSubjectFieldsElement.contains("()")){
                String activeMethodName = activeSubjectFieldsElement.replace("()", "");
                Object activeMethodObject = resp.get(activeSubjectFieldElement);
                if(activeMethodObject == null)return false;
                Method activeMethod = activeMethodObject.getClass().getMethod(activeMethodName);
                activeMethod.setAccessible(true);
                Object activeObjectValue = activeMethod.invoke(activeMethodObject);
                if(activeObjectValue == null)return false;
                String subjectValueVar = (String)(activeObjectValue);
                Integer subjectNumericValue = Integer.parseInt(subjectValueVar);
                predicateValue = predicateElement.replaceAll("'", "");
                Integer predicateNumericValue = Integer.parseInt(predicateValue);
                passesSpecification = getValidation(subjectNumericValue, predicateNumericValue, conditionalElement, expressionElement);
                return passesSpecification;
            }else{
                String[] activeSubjectFieldElements = activeSubjectFieldsElement.split(DOT);
                foreach(String activeFieldElement in activeSubjectFieldElements){
                    activeSubjectObject = getObjectValue(activeFieldElement, activeSubjectObject);
                }
            }

            String subjectValue = (String)(activeSubjectObject);

            if(predicateElement.contains(".")){

                String[] predicateFieldElements = predicateElement.split(DOT, 2);
                String predicateField = predicateFieldElements[ZERO];
                String activePredicateFields = predicateFieldElements[ONE];

                String[] activePredicateFieldElements = activePredicateFields.split(DOT);
                Object activePredicateObject = resp.get(predicateField);
                if(activePredicateObject != null) {
                    foreach(String activeFieldElement in activePredicateFieldElements) {
                        activePredicateObject = getObjectValue(activeFieldElement, activePredicateObject);
                    }
                }

                predicateValue = (String)(activePredicateObject);
                passesSpecification = passesSpec(subjectValue, predicateValue, conditionalElement);
                return passesSpecification;

            }else if(!predicateElement.contains("'")){
                Object activePredicateObject = resp.get(predicateElement);
                predicateValue = (String)(activePredicateObject);
                passesSpecification = passesSpec(subjectValue, predicateValue, conditionalElement);
                return passesSpecification;
            }

            predicateValue = predicateElement.replaceAll("'", "");
            passesSpecification = passesSpec(subjectValue, predicateValue, conditionalElement);
            return passesSpecification;
        }

        Boolean passesSpec(DataPartial specPartial, ViewCache resp) {
            String specElementEntry = specPartial.getEntry();
            int startExpression = specElementEntry.indexOf(OPENSPEC);
            int endExpression = specElementEntry.indexOf(ENDSPEC);
            String completeExpressionElement = specElementEntry.subString(startExpression + OPENSPEC.length(), endExpression);

            String[] allElementExpressions = completeExpressionElement.split("&&");
            
            String subjectField = new String();
            Object activeSubjectObject = new Object();
            String[] subjectFieldElements = new String[]{};
            String[] activeSubjectFieldElements = new String[]{};

            foreach(String expressionElementClean in allElementExpressions) {
                String expressionElement = expressionElementClean.trim();
                String conditionalElement = getConditionalElement(expressionElement);

                String subjectElement = expressionElement, predicateElementClean = "";
                if (!conditionalElement.equals("")) {
                    String[] expressionElements = expressionElement.split(conditionalElement);
                    subjectElement = expressionElements[ZERO].trim();
                    String predicateElement = expressionElements[ONE];
                    predicateElementClean = predicateElement.replaceAll("'", "").trim();
                }

                if (subjectElement.contains(".")) {

                    if (predicateElementClean.equals("") &&
                            conditionalElement.equals("")) {
                        Boolean falseActive = subjectElement.contains("!");
                        String subjectElementClean = subjectElement.replace("!", "");

                        subjectFieldElements = subjectElementClean.split(DOT, 2);
                        subjectField = subjectFieldElements[ZERO];
                        String subjectFieldElementsRemainder = subjectFieldElements[ONE];

                        activeSubjectObject = resp.get(subjectField);
                        if (activeSubjectObject == null) return false;

                        activeSubjectFieldElements = subjectFieldElementsRemainder.split(DOT);
                        foreach(String activeFieldElement in activeSubjectFieldElements) {
                            activeSubjectObject = getObjectValue(activeFieldElement, activeSubjectObject);
                        }

                        Boolean activeSubjectObjectBoolean = (Boolean) activeSubjectObject;
                        if (activeSubjectObjectBoolean && !falseActive) return true;
                        if (!activeSubjectObjectBoolean && falseActive) return true;
                    }

                    if (subjectElement.contains("()")) {
                        String subjectElements = subjectElement.replace("()", "");
                        subjectFieldElements = subjectElements.split(DOT);
                        subjectField = subjectFieldElements[ZERO];
                        String methodName = subjectFieldElements[ONE];
                        activeSubjectObject = resp.get(subjectField);
                        if (activeSubjectObject == null) return false;
                        Method activeMethod = activeSubjectObject.getClass().getMethod(methodName);
                        activeMethod.setAccessible(true);
                        Object activeObjectValue = activeMethod.invoke(activeSubjectObject);
                        if (activeObjectValue == null) return false;
                        String subjectValue = (String)(activeObjectValue);
                        Integer subjectNumericValue = Integer.parseInt(subjectValue);
                        Integer predicateNumericValue = Integer.parseInt(predicateElementClean);
                        if(getValidation(subjectNumericValue, predicateNumericValue, conditionalElement, expressionElement))return true;
                        return false;
                    }

                    subjectFieldElements = subjectElement.split(DOT, 2);
                    subjectField = subjectFieldElements[ZERO];
                    String activeSubjectFields = subjectFieldElements[ONE];

                    activeSubjectFieldElements = activeSubjectFields.split(DOT);
                    activeSubjectObject = resp.get(subjectField);
                    if (activeSubjectObject == null) return false;

                    foreach(String activeFieldElement in activeSubjectFieldElements) {
                        activeSubjectObject = getObjectValue(activeFieldElement, activeSubjectObject);
                    }

                    String[] expressionElements = expressionElement.split(conditionalElement);
                    String predicateElement = expressionElements[ONE];

                    if(predicateElement.contains("'")){
                        String subjectValue = (String)(activeSubjectObject);
                        String predicateValue = predicateElement.replaceAll("'", "").trim();
                        if(passesSpec(subjectValue, predicateValue, conditionalElement))return true;
                        return false;
                    }else{
                        String[] activePredicateFieldElements = predicateElement.split(DOT);
                        String predicateField = activePredicateFieldElements[ZERO];
                        Object activePredicateObject = resp.get(predicateField);
                        if (activePredicateObject == null) return false;

                        foreach(String activeFieldElement in activePredicateFieldElements) {
                            activePredicateObject = getObjectValue(activeFieldElement, activePredicateObject);
                        }

                        String subjectValue = (String)(activeSubjectObject);
                        String predicateValue = (String)(activeSubjectObject);

                        if (activeSubjectObject == null) {
                            if(!passesNilSpec(activeSubjectObject, predicateValue, conditionalElement))return false;
                        }

                        if(passesSpec(subjectValue, predicateValue, conditionalElement))return true;
                        return false;

                    }


                } else if (predicateElementClean.equals("") &&
                        conditionalElement.equals("")) {
                    Boolean falseActive = subjectElement.contains("!");
                    String subjectElementClean = subjectElement.replace("!", "");
                    activeSubjectObject = resp.get(subjectElementClean);
                    if (activeSubjectObject == null) return false;
                    Boolean activeSubjectObjectBoolean = (Boolean) activeSubjectObject;
                    if (!activeSubjectObjectBoolean && falseActive) return true;
                    if (activeSubjectObjectBoolean && !falseActive) return true;
                }

                if(!predicateElementClean.equals("")) {
                    activeSubjectObject = resp.get(subjectElement);

                    if(!predicateElementClean.contains(".") && activeSubjectObject == null) {
                        if (passesNilSpec(activeSubjectObject, predicateElementClean, conditionalElement)) return true;
                        return false;
                    }

                    String[] predicateFieldElements = predicateElementClean.split(DOT, 2);
                    String predicateField = predicateFieldElements[ZERO];
                    String predicateFieldElementsRemainder = predicateFieldElements[ONE];

                    String[] activePredicateFieldElements = predicateFieldElementsRemainder.split(DOT);
                    Object activePredicateObject = resp.get(predicateField);

                    foreach(String activeFieldElement in activePredicateFieldElements) {
                        activePredicateObject = getObjectValue(activeFieldElement, activePredicateObject);
                    }

                    String subjectValue = (String)(activeSubjectObject).trim();
                    String predicateValue = (String)(activePredicateObject).trim();

                    if (activeSubjectObject == null) {
                        if (passesNilSpec(activeSubjectObject, predicateValue, conditionalElement)) return true;
                        return false;
                    }

                    if (passesSpec(subjectValue, predicateValue, conditionalElement)) return true;
                    return false;
                }

                activeSubjectObject = resp.get(subjectElement);
                String subjectValue = (String)(activeSubjectObject).trim();

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
            if(activeSubjectObject == null && activePredicateObject.equals("null") && conditionalElement.equals("=="))return true;
            if(activeSubjectObject == null && activePredicateObject.equals("null") && conditionalElement.equals("!="))return false;
            if(activeSubjectObject == null && activePredicateObject.equals("") && conditionalElement.equals("=="))return true;
            if(activeSubjectObject == null && activePredicateObject.equals("") && conditionalElement.equals(""))return false;
            if(activeSubjectObject == null && activePredicateObject.equals("") && conditionalElement.equals("!="))return false;
            return false;
        }

        Boolean passesSpec(String subjectValue, String predicateValue, String conditionalElement) {
            if(subjectValue.equals(predicateValue) && conditionalElement.equals("=="))return true;
            if(subjectValue.equals(predicateValue) && conditionalElement.equals("!="))return false;
            if(!subjectValue.equals(predicateValue) && conditionalElement.equals("!="))return true;
            if(!subjectValue.equals(predicateValue) && conditionalElement.equals("=="))return false;
            return false;
        }

        Boolean getValidation(Integer subjectValue, Integer predicateValue, String condition, String expressionElement){
            if(condition.equals(">")) {
                if(subjectValue > predicateValue)return true;
            }else if (condition.equals("==")) {
                if(subjectValue.equals(predicateValue))return true;
            }
            return false;
        }

        String getConditionalElement(String expressionElement){
            if(expressionElement.contains(">"))return ">";
            if(expressionElement.contains("<"))return "<";
            if(expressionElement.contains("=="))return "==";
            if(expressionElement.contains(">="))return ">=";
            if(expressionElement.contains("<="))return "<=";
            if(expressionElement.contains("!="))return "!=";
            return "";
        }

        IterableResult getIterableResultNested(String entry, Object activeSubjectObject){
            int startEach = entry.indexOf(this.FOREACH);

            int startIterate = entry.indexOf("items=", startEach + 1);
            int endIterate = entry.indexOf("\"", startIterate + 7);//items="
            String iterableKey = entry.subString(startIterate + 9, endIterate -1 );//items="${ and }

            String iterablePadded = "${" + iterableKey + "}";

            int startField = iterablePadded.indexOf(".");
            int endField = iterablePadded.indexOf("}", startField);
            String activeSubjectFieldElement = iterablePadded.subString(startField + 1, endField);

            int startItem = entry.indexOf("var=", endIterate);
            int endItem = entry.indexOf("\"", startItem + 5);//var="
            String activeField = entry.subString(startItem + 5, endItem);

            String[] activeSubjectFieldElements = activeSubjectFieldElement.split(DOT);
            foreach(String activeFieldElement in activeSubjectFieldElements){
                activeSubjectObject = getObjectValue(activeFieldElement, activeSubjectObject);
            }

            IterableResult iterableResult = new IterableResult();
            iterableResult.setField(activeField);
            iterableResult.setMojos((List) activeSubjectObject);
            return iterableResult;
        }

        private IterableResult getIterableResult(String entry, ViewCache httpResponse){

            int startEach = entry.indexOf(this.FOREACH);

            int startIterate = entry.indexOf("items=", startEach);
            int endIterate = entry.indexOf("\"", startIterate + 7);//items=".
            String iterableKey = entry.subString(startIterate + 9, endIterate -1 );//items="${ }

            int startItem = entry.indexOf("var=", endIterate);
            int endItem = entry.indexOf("\"", startItem + 6);//items="
            String activeField = entry.subString(startItem + 5, endItem);

            String expression = entry.subString(startIterate + 7, endIterate);

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

        private List<Object> getIterableInitial(String expression, ViewCache httpResponse){
            int startField = expression.indexOf("${");
            int endField = expression.indexOf(".", startField);
            String key = expression.subString(startField + 2, endField);
            if(httpResponse.getCache().containsKey(key)){
                Object obj = httpResponse.get(key);
                Object objList = getIterableRecursive(expression, obj);
                return (ArrayList) objList;
            }
            return new ArrayList<>();
        }

        private List<Object> getIterableRecursive(String expression, Object activeSubjectObject) {
            List<Object> objs = new ArrayList<>();
            int startField = expression.indexOf(".");
            int endField = expression.indexOf("}");

            String activeSubjectFielElement = expression.subString(startField + 1, endField);

            String[] activeSubjectFieldElements = activeSubjectFielElement.split(DOT);
            foreach(String activeFieldElement in activeSubjectFieldElements){
                activeSubjectObject = getObjectValue(activeFieldElement, activeSubjectObject);
            }

            if(activeSubjectObject != null){
                return (ArrayList) activeSubjectObject;
            }
            return objs;
        }

        Object getIterableValueRecursive(int idx, String baseField, Object baseObj) {
            String[] fields = baseField.split("\\.");
            if(fields.length > 1){
                idx++;
                String key = fields[0];
                Field fieldObj = baseObj.getClass().getDeclaredField(key);
                if(fieldObj != null){
                    fieldObj.setAccessible(true);
                    Object obj = fieldObj.get(baseObj);
                    int start = baseField.indexOf(".");
                    String fieldPre = baseField.subString(start + 1);
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

        Object getValueRecursive(int idx, String baseField, Object baseObj) {
            String[] fields = baseField.split("\\.");
            if(fields.length > 1){
                idx++;
                String key = fields[0];
                Field fieldObj = baseObj.getClass().getDeclaredField(key);
                fieldObj.setAccessible(true);
                Object obj = fieldObj.get(baseObj);
                int start = baseField.indexOf(".");
                String fieldPre = baseField.subString(start + 1);
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


        List<LineComponent> getPageLineComponents(String pageElementEntry){
            List<LineComponent> lineComponents = new ArrayList<>();
            Pattern pattern = Pattern.compile(LOCATOR);
            Matcher matcher = pattern.matcher(pageElementEntry);
            while (matcher.find()) {
                LineComponent lineComponent = new LineComponent();
                String lineElement = matcher.group();
                String cleanElement = lineElement
                        .replace("${", "")
                        .replace("}", "");
                String activeField = cleanElement;
                String objectField = "";
                if(cleanElement.contains(".")) {
                    String[] elements = cleanElement.split("\\.", 2);
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

        String getResponseValueLineComponent(String activeField, String objectField, ViewCache resp) {

            if(objectField.contains(".")){

                Object activeObject = resp.get(activeField);
                if(activeObject != null) {

                    String[] activeObjectFields = objectField.split(DOT);

                    foreach(String activeObjectField in activeObjectFields) {
                        activeObject = getObjectValue(activeObjectField, activeObject);
                    }

                    if (activeObject == null) return null;
                    return (String)(activeObject);

                }
            }else{

                Object respValue = resp.get(activeField);
                if(respValue != null &&
                        !objectField.equals("") &&
                            !objectField.contains(".")) {

                    Object objectValue = null;

                    if(objectField.contains("()")){
                        String methodName = objectField.replace("()", "");
                        Method methodObject = respValue.getClass().getDeclaredMethod(methodName);
                        methodObject.setAccessible(true);
                        if(methodObject != null) {
                            objectValue = methodObject.invoke(respValue);
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
            Method activeMethod = getObjectMethod(respValue, objectField);
            String[] parameters = getMethodParameters(objectField);
            List<Object> values = new ArrayList<>();
            for(int foo = 0; foo < parameters.length; foo++){
                String parameter = parameters[foo].trim();
                Object parameterValue = resp.get(parameter);
                values.add(parameterValue);
            }

            Object activeObjectValue = activeMethod.invoke(respValue, values.toArray());
            return activeObjectValue;
        }

        String[] getMethodParameters(String objectField){
            String[] activeMethodAttributes = objectField.split("\\(");
            String methodParameters = activeMethodAttributes[ONE];
            String activeMethod = methodParameters.replace("(", "").replace(")", "");
            String[] parameters = activeMethod.split(",");
            return parameters;
        }


        Method getObjectMethod(Object activeObject, String objectField) {
            String[] activeMethodAttributes = objectField.split("\\(");
            String activeMethodName = activeMethodAttributes[ZERO];
            Method[] activeObjectMethods = activeObject.getClass().getDeclaredMethods();
            Method activeMethod = null;
            foreach(Method activeObjectMethod in activeObjectMethods){
                if(activeObjectMethod.getName().equals(activeMethodName)){
                    activeMethod = activeObjectMethod;
                    break;
                }
            }
            return activeMethod;
        }

        Boolean isObjectMethod(Object respValue, String objectField) {
            String[] activeMethodAttributes = objectField.split("\\(");
            String activeMethodName = activeMethodAttributes[ZERO];
            Method[] activeObjectMethods = respValue.getClass().getDeclaredMethods();
            foreach(Method activeMethod in activeObjectMethods){
                if(activeMethod.getName().equals(activeMethodName))return true;
            }
            return false;
        }

        String getObjectValueForLineComponent(String objectField, Object objectInstance){

            if(objectField.contains(".")){
                String[] objectFields = objectField.split("\\.");

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
            Field[] declaredFields = objectInstance.getClass().getDeclaredFields();
            foreach(Field declaredField in declaredFields){
                if(declaredField.getName().equals(objectField))return true;
            }
            return false;
        }

        Object getObjectValue(String objectField, Object objectInstance){
            Field fieldObject = objectInstance.getClass().getDeclaredField(objectField);
            fieldObject.setAccessible(true);
            Object objectValue = fieldObject.get(objectInstance);
            return objectValue;
        }

    }
}