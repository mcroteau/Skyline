<c:if spec="${message != ''}">
    <p>${message}</p>
</c:if>

<ul>
    <c:foreach items="${items}" var="item">
        <li>${item.description}</li>
    </c:foreach>
</ul>