<h1>Edit User # ${user.id}</h1>
<form action="/users/update/${user.id}" method="post">
    <label>Email</label>
    <input type="text" name="email" value="${user.email}"/>

    <label>Password</label>
    <input type="text" name="password" value="${user.password}"/>
    <input type="submit" value="update"/>
</form>