<html>
    <head>
        <title>${headline}</title>
        <link rel="stylesheet" href="/assets/Default.css">
    </head>
    <body>
        <c:if spec="${sessionuser == ''}">
            <a href="/signin">Signin</a>
        </c:if>
        <c:if spec="${sessionuser != ''}">
            welcome ${sessionuser} <a href="/signout">Signout</a>
        </c:if>
        
        <a href="/users/create">Create User</a> <a href="/users">List Users</a>

        <c:if spec="${message != ''}">
            <p class="alert">${message}</p>
        </c:if>

        <c:content/>
    </body>
</html>