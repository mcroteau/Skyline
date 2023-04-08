<c:if spec="${message != ''}">
    <p>${message}</p>
</c:if>

<h1>Users</h1>
<ul>
    <c:foreach items="${users}" var="user">
        <li>
            ${user.email} <a href="/users/edit/${user.id}">Edit</a>
        </li>
    </c:foreach>
</ul>