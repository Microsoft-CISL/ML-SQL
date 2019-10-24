from sqlalchemy import create_engine


class DatabaseConnectionManager(object):

    def __init__(self):
        self.host = "localhost"

    def get_string_db_connection(self, dbms, database, charset=None):

        if dbms == "mysql":
            self.username = "root"
            #self.password = "ML+matteo<3paolo+SQL"
            self.password = "ndulsp+92+pgnll"
            dialect = 'mysql+pymysql'

            # if charset:
            #    db_string = '{}://{}:{}@{}/{}?charset={}'.format(dialect, self.username, self.password, self.host, database, charset)
            # else:
            db_string = '{}://{}:{}@{}/{}'.format(dialect, self.username, self.password, self.host, database)
        elif dbms == "sqlserver":
            self.username = "SA"
            self.password = "ML+matteo<3paolo+SQL"
            # pymssql
            dialect = 'mssql+pymssql'

            db_string = '{}://{}:{}@{}/{}'.format(dialect, self.username, self.password, self.host, database)
        else:
            raise ValueError("DBMS not supported. Only 'mysql' and 'sqlserver' are supported.")

        #if charset:
        #    db_string = '{}://{}:{}@{}/{}?charset={}'.format(dialect, self.username, self.password, self.host, database, charset)
        #else:
        #    db_string = '{}://{}:{}@{}/{}'.format(dialect, self.username, self.password, self.host, database)

        return db_string

    def create_db_connection(self, dbms, database, charset=None):
        string_db_conn = self.get_string_db_connection(dbms, database, charset=charset)

        if dbms == "mysql":
            engine = create_engine(string_db_conn, pool_recycle=3600)
        else:
            engine = create_engine(string_db_conn)

        connection = engine.connect()

        return connection

